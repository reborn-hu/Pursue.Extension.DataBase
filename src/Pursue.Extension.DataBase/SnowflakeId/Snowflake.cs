using Pursue.Extension.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pursue.Extension.DataBase.SnowflakeId
{
    public sealed class Snowflake : ISnowflake
    {
        #region 声明

        /// <summary>
        /// 基准时间
        /// </summary>
        private const long Twepoch = 1288834974657L;

        /// <summary>
        /// 序列号位数
        /// </summary>
        private const int SequenceBits = 12;

        /// <summary>
        /// 机器码位数
        /// </summary>
        private const int WorkerIdBits = 10;

        /// <summary>
        /// 序列号最大值
        /// </summary>
        private const long Max_SequenceId = -1L ^ -1L << SequenceBits;

        /// <summary>
        /// 机器码左移12位
        /// </summary>
        private const int WorkerIdShift = SequenceBits;

        /// <summary>
        /// 时间戳左移22位
        /// </summary>
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits;

        /// <summary>
        /// 序列存储由程序自动累加，位数(0~4095)
        /// </summary>
        private long _sequence = 0L;

        /// <summary>
        /// 时间戳记录值
        /// </summary>
        private long _lastTimestamp = -1L;

        public const string HashRedisKey = "Snowflake:WorkerIdHash";

        public const string RedisLockKey = "Snowflake:WorkerId:Lock";

        public const int RedisLockTimeout = 30;

        public const int LocalCacheTimeout = 600;

        /// <summary>
        /// 工作机器ID，防止多机器冲撞问题
        /// </summary>
        private readonly long _workerId = 0L;

        private static RedisClient _redisClient;
        private static MemoryClient _memoryClient;

        #endregion

#if NET6_0 || NET8
        private readonly object _lock = new object();
        private readonly object _lock_id = new object();
#endif
#if NET9_0
        private readonly Lock _lock = new Lock();
        private readonly Lock _lock_id = new Lock();
#endif

        public Snowflake()
        {
            if (ConnectionOptions.RedisWorkeIdEnable)
            {
                _redisClient = CacheFactory.GetRedisClient();
                _memoryClient = CacheFactory.GetMemoryClient();

                _workerId = GetWorkerIdDataCircleBus();
            }
            else
            {
                _workerId = ConnectionOptions.DefaultWorkerId == default ? new Random().Next(1, 1023) : ConnectionOptions.DefaultWorkerId;
            }
        }

        public string GetIdToString()
        {
            return GetId().ToString();
        }

        public long GetId()
        {
            try
            {
#if NET6_0 || NET8
                lock (_lock_id)
#endif
#if NET9_0 
                lock (_lock_id)
#endif
                {
                    // 机器码，位数（0~1023） 获取WorkerId
                    var workerId = _workerId;
                    var timestamp = UnixTime;
                    if (timestamp < _lastTimestamp)
                    {
                        throw new ApplicationException();
                    }
                    //如果上次生成时间和当前时间相同,在同一毫秒内
                    if (_lastTimestamp == timestamp)
                    {
                        //sequence自增，和sequenceMask相与一下，去掉高位
                        _sequence = _sequence + 1 & Max_SequenceId;
                        //判断是否溢出,也就是每毫秒内超过4096，当为4096时，与sequenceMask相与，sequence就等于0
                        if (_sequence == 0)
                        {
                            //等待到下一毫秒
                            timestamp = TilNextMillis(_lastTimestamp);
                        }
                    }
                    else
                    {
                        _sequence = 0;
                    }

                    _lastTimestamp = timestamp;

                    return timestamp - Twepoch << TimestampLeftShift | workerId << WorkerIdShift | _sequence;
                }
            }
            catch (ApplicationException)
            {
                Thread.Sleep(1);
                return GetId();
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 循环时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private static long TilNextMillis(long lastTimestamp)
        {
            var timestamp = UnixTime;
            while (timestamp <= lastTimestamp)
            {
                timestamp = UnixTime;
            }
            return timestamp;
        }

        /// <summary>
        /// 获取unix时间戳
        /// </summary>
        /// <returns></returns>
        private static long UnixTime => (DateTime.UtcNow.Ticks - 621355968000000000) / 10000;

        /// <summary>
        /// 获取WorkerId
        /// </summary>
        /// <returns></returns>
        private long GetWorkerIdDataCircleBus()
        {
            try
            {

                var _dataCircleBus = new DataCircleBus<string>
                {
                    GetLocalData = () =>
                    {
                        // 获取本地缓存的workerId
                        var workerId = _memoryClient.Get<string>($"{HashRedisKey}:{ConnectionOptions.ServerName}:{ConnectionOptions.ServerIP}");

                        return string.IsNullOrEmpty(workerId) ? null : workerId;
                    },

                    GetRedisData = () =>
                    {
                        // 匹配规则  系统类型:服务器IP  XXX:192.1.1.1
                        var serverKey = $"{ConnectionOptions.ServerName}:{ConnectionOptions.ServerIP}";

                        // 获取固定Hash表  dataKey 0~1023  value 匹配对应机器_应用
                        var data = _redisClient.HGetAll<string>(HashRedisKey);
                        // 获取对应该应用的workerId
                        var workerId = data?.FirstOrDefault(o => o.Value.Equals(serverKey)).Key;
                        if (data != null && data.Count > 0 && !string.IsNullOrEmpty(workerId))
                        {
                            return workerId;
                        }
                        else
                        {
                            using (var redisLock = _redisClient.TryLock(RedisLockKey, RedisLockTimeout))
                            {
                                try
                                {
                                    data = new Dictionary<string, string>();

                                    _redisClient.StartPipe(redis =>
                                    {
                                        for (int i = 0; i < 1024; i++)
                                        {
                                            data.Add(i.ToString(), "");
                                            redis.HSet(HashRedisKey, i.ToString(), "");
                                        }
                                    });

                                    // 取一个空白Id
                                    workerId = data.FirstOrDefault(o => string.IsNullOrEmpty(o.Value)).Key;

                                    // 写入对应关系
                                    _redisClient.HSet(HashRedisKey, workerId, serverKey);
                                }
                                catch
                                {
                                    workerId = new Random().Next(0, 1023).ToString();
                                    redisLock.Unlock();
                                }
                            }
                        }
                        return workerId;
                    },

                    SetLocalData = (value) =>
                    {
                        // 获取服务器IP
                        var localKey = $"{HashRedisKey}:{ConnectionOptions.ServerName}:{ConnectionOptions.ServerIP}";

                        if (!_memoryClient.TryGetValue(localKey, out string workerId))
                        {
#if NET6_0 || NET8
                            lock (_lock_id)
#endif
#if NET9_0
                            lock (_lock_id)
#endif
                            {
                                if (!_memoryClient.TryGetValue(localKey, out workerId))
                                {
                                    _memoryClient.Set(localKey, value, LocalCacheTimeout);
                                }
                            }
                        }
                    }
                };

                // 执行组合好的委托
                return Convert.ToInt64(_dataCircleBus.GetData(_dataCircleBus));
            }
            catch
            {
                return ConnectionOptions.DefaultWorkerId == default ? new Random().Next(0, 1023) : ConnectionOptions.DefaultWorkerId;
            }
        }
    }
}