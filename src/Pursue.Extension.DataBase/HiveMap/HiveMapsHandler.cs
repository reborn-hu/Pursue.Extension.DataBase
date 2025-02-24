using MapsterMapper;
using Microsoft.Extensions.Logging;
using Pursue.Extension.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pursue.Extension.DataBase.HiveMap
{
    public sealed class HiveMapsHandler
    {
        #region 声明

        /// <summary>
        /// HiveMaps连接字符串Redis缓存
        /// </summary>
        internal const string HiveMapsKey = "hivemaps";
        /// <summary>
        /// HiveMaps分布式锁
        /// </summary>
        internal const string HiveMapsLockKey = "hivemaps:lock";
        /// <summary>
        /// HiveMaps连接字符串Redis缓存有效时间
        /// </summary>
        internal const int HiveMapsExpireKey = 3600;
        /// <summary>
        /// HiveMaps分布式锁超时
        /// </summary>
        internal const int HiveMapsLockTimeoutKey = 30;
        /// <summary>
        /// HiveMaps本地缓存有效时间
        /// </summary>
        internal const int LocalCacheTimeout = 60;

        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly RedisClient _redisClient;
        private readonly MemoryClient _memoryClient;

        #endregion

#if NET6_0 || NET8_0
        private readonly object _lock = new object();
#endif

#if NET9_0
        private readonly Lock _lock = new Lock();
#endif

        public HiveMapsHandler(ILogger<HiveMapsHandler> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;

            _redisClient = CacheFactory.GetRedisClient();
            _memoryClient = CacheFactory.GetMemoryClient();
        }

        /// <summary>
        /// 分库连接
        /// </summary>
        /// <param name="tenantId">租户编码</param>
        /// <param name="businessType">数据库业务类型</param>
        /// <param name="commadType">数据库执行类型</param>
        /// <param name="connectionStringCipher">解密连接字符串的函数</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">传入数据库执行类型错误</exception>
        public string GetConnectString(string tenantId, DbBusinessType businessType, DbCommadType commadType, Func<string, string> connectionStringCipher = default)
        {
            var connectString = GetHiveMapsSource(tenantId, businessType, commadType);
            if (!string.IsNullOrEmpty(connectString))
            {
                if (ConnectionOptions.DbCipherType == DbCipherType.plaintext || connectionStringCipher == default)
                {
                    return connectString;
                }
                else
                {
                    return connectionStringCipher(connectString);
                }
            }
            return default;
        }

        private string GetHiveMapsSource(string tenantId, DbBusinessType businessType, DbCommadType commadType)
        {
            // 使用分库配置
            if (ConnectionOptions.ShardingEnable)
            {
                var hiveMaps = GetHiveMapsDataCircleBus(tenantId);
                if (hiveMaps != default)
                {
                    var hivemap = businessType switch
                    {
                        DbBusinessType.Base => hiveMaps.First(o => o.Type == DbBusinessType.Base),
                        DbBusinessType.Biz => hiveMaps.First(o => o.Type == DbBusinessType.Biz),
                        DbBusinessType.Logger => hiveMaps.First(o => o.Type == DbBusinessType.Logger),
                        _ => throw new ArgumentNullException(nameof(DbBusinessType).ToString(), "数据库业务类型错误!")
                    };
                    if (hivemap != default)
                    {
                        return commadType switch
                        {
                            DbCommadType.Write => hivemap.Write,
                            DbCommadType.Read => hivemap.Read,
                            _ => throw new ArgumentNullException(nameof(DbCommadType).ToString(), "数据库执行类型错误!"),
                        };
                    }
                }

                throw new ArgumentNullException(nameof(commadType).ToString(), $"租户:{tenantId}, 未配置分库!");
            }
            else
            {
                return businessType switch
                {
                    DbBusinessType.Base => ConnectionOptions.BaseConnectString,
                    DbBusinessType.Biz => ConnectionOptions.BizConnectString,
                    DbBusinessType.Logger => ConnectionOptions.LogConnectString,
                    _ => throw new ArgumentNullException(nameof(DbCommadType).ToString(), "数据库执行类型错误!"),
                };
            }
        }

        /// <summary>
        /// 获取HiveMaps循环总线
        /// </summary>
        /// <param name="tenantId">租户编码</param>
        /// <returns></returns>
        private List<HiveMapsModel> GetHiveMapsDataCircleBus(string tenantId)
        {
            try
            {
                var _dataCircleBus = new DataCircleBus<List<HiveMapsModel>>
                {
                    GetLocalData = () =>
                    {
                        var result = _memoryClient.Get<Dictionary<string, HiveMapsModel>>($"{HiveMapsKey}:{tenantId}");
                        return result != null && result.Count != 0 ? result.Select(o => o.Value).ToList() : null;
                    },
                    GetRedisData = () =>
                    {
                        var result = _redisClient.HVals<HiveMapsModel>($"{HiveMapsKey}:{tenantId}");
                        return result != null && result.Length != 0 ? result.ToList() : null;
                    },
                    SetLocalData = (value) =>
                    {
                        var localKey = $"{HiveMapsKey}:{tenantId}";
                        if (!_memoryClient.TryGetValue(localKey, out Dictionary<string, HiveMapsModel> hiveMapsDic))
                        {
                            lock (_lock)
                            {
                                _memoryClient.Set(localKey, value.ToDictionary(o => o.Type.ToString(), o => o), LocalCacheTimeout);
                            }
                        }
                    },
                    SetRedisData = (value) =>
                    {
                        using var redisLock = _redisClient.TryLock($"{HiveMapsLockKey}:{tenantId}", HiveMapsLockTimeoutKey);
                        try
                        {
                            _redisClient.StartPipe(options =>
                            {
                                foreach (var item in value)
                                {
                                    options.HSet($"{HiveMapsKey}:{tenantId}", item.Type.ToString(), item);
                                }
                            });
                        }
                        catch
                        {
                            redisLock.Unlock();
                        }
                    },
                    GetDatabaseData = () =>
                    {
                        return GetDataBaseHiveMaps(tenantId);
                    }
                };
                return _dataCircleBus.GetData(_dataCircleBus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "方法: HiveMapsFactory.GetHiveMapsDataCircleBus(string tenantId) 执行错误!");

                return GetDataBaseHiveMaps(tenantId);
            }
        }

        public List<HiveMapsModel> GetDataBaseHiveMaps(string tenantId)
        {
            var result = ORM.SwitchDbContext(DbBusinessType.Base.GetDescription(), DbBusinessType.Base, ConnectionOptions.BaseConnectString)
                        .Queryable<HiveMapsEntity>()
                        .Where(o => o.TenantId == tenantId)
                        .ToList();

            return _mapper.Map<List<HiveMapsModel>>(result);
        }
    }
}