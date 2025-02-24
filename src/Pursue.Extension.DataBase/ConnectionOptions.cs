using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace Pursue.Extension.DataBase
{
    public class ConnectionOptions
    {
        #region 数据库配置

        /// <summary>
        /// 是否使用分库配置
        /// <para>默认: false</para>
        /// </summary>
        public static bool ShardingEnable { get; private set; } = false;

        /// <summary>
        /// 数据库类型
        /// <para>默认：MySql</para>
        /// </summary>
        public static DbType DbType { get; private set; } = DbType.MySql;

        /// <summary>
        /// 数据库连接串加密模式
        /// <para>默认：plaintext</para>
        /// </summary>
        public static DbCipherType DbCipherType { get; private set; } = DbCipherType.plaintext;

        /// <summary>
        /// 基础库连接
        /// </summary>
        public static string BaseConnectString { get; private set; }

        /// <summary>
        /// 业务库连接
        /// </summary>
        public static string BizConnectString { get; private set; }

        /// <summary>
        /// 日志库连接
        /// </summary>
        public static string LogConnectString { get; private set; }

        /// <summary>
        /// 数据库还原类
        /// </summary>
        public static Type MigratorType { get; private set; }

        #endregion

        #region 雪花ID配置

        /// <summary>
        /// 服务名称
        /// </summary>
        public static string ServerName { get; private set; }

        /// <summary>
        /// 服务器IP
        /// </summary>
        public static string ServerIP { get; private set; }

        /// <summary>
        /// 雪花Id，工作区编码（防冲撞）
        /// </summary>
        public static bool RedisWorkeIdEnable { get; private set; } = false;

        /// <summary>
        /// 默认工作区编码
        /// <para>默认: 1L</para>
        /// </summary>
        public static long DefaultWorkerId { get; private set; }

        #endregion

        #region 资源还原配置

        /// <summary>
        /// 是否开启还原数据库
        /// </summary>
        public static bool MigratorEnable { get; private set; } = false;

        /// <summary>
        /// 数据库还原连接
        /// </summary>
        public static string MigratorConnectString { get; private set; }

        #endregion

        /// <summary>
        /// 指定默认配置数据连接字符串
        /// </summary>
        /// <param name="configuration">配置文件</param>
        /// <param name="section">配置文件节点</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">数据库连接配置异常</exception>
        public ConnectionOptions UseConnectionOptions(IConfiguration configuration, string section = "Configuration:Data")
        {
            var root = configuration.GetSection(section).Get<DataBaseSettingsRoot>() ?? throw new NullReferenceException("请配置数据库连接配置");

            ShardingEnable = root.ShardingEnable;

            if (Enum.TryParse(root.DbConnectStringCipherType, out DbCipherType connectStringCipherType))
            {
                DbCipherType = connectStringCipherType;
            }
            if (Enum.TryParse(root.DataBaseType, out DbType dbStore))
            {
                DbType = dbStore;
            }
            if (root.ConnectionSettings.TryGetValue(DbBusinessType.Base.GetDescription(), out string baseConnectString) && !string.IsNullOrEmpty(baseConnectString))
            {
                BaseConnectString = baseConnectString.DecodeConnectString();
            }
            if (root.ConnectionSettings.TryGetValue(DbBusinessType.Biz.GetDescription(), out string bizConnectString) && !string.IsNullOrEmpty(bizConnectString))
            {
                BizConnectString = bizConnectString.DecodeConnectString();
            }
            if (root.ConnectionSettings.TryGetValue(DbBusinessType.Logger.GetDescription(), out string loggerConnectString) && !string.IsNullOrEmpty(loggerConnectString))
            {
                LogConnectString = loggerConnectString.DecodeConnectString();
            }

            return this;
        }

        /// <summary>
        /// 设置雪花ID配置
        /// </summary>
        /// <param name="configuration">配置文件</param>
        /// <param name="section">配置文件节点</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public ConnectionOptions UseSnowflakeOptions(IConfiguration configuration, string section = "Configuration:Snowflake")
        {
            var root = configuration.GetSection(section).Get<DataBaseSettingsRoot>() ?? throw new NullReferenceException("请正确配置雪花Id配置");

            ServerName = new Random().Next(100000, 999999).ToString();
            ServerIP = new Random().Next(100000, 999999).ToString();

            RedisWorkeIdEnable = root.RedisWorkeIdEnable;
            DefaultWorkerId = new Random().Next(100000, 999999);

            return this;
        }

        /// <summary>
        /// 设置数据库还原配置
        /// </summary>
        /// <param name="migratorType">指定还原配置类</param>
        /// <param name="ConnectString">指定还原数据库连接串</param>
        /// <returns></returns>
        public ConnectionOptions UseMigratorOptions(Type migratorType, string ConnectString)
        {
            MigratorEnable = true;
            MigratorType = migratorType;
            MigratorConnectString = ConnectString;

            return this;
        }
    }

    sealed class DataBaseSettingsRoot
    {
        /// <summary>
        /// 是否启用分库配置
        /// </summary>
        public bool ShardingEnable { get; set; }

        /// <summary>
        /// 是否开启二级缓存
        /// </summary>
        public bool L2CacheEnable { get; set; }

        /// <summary>
        /// 是否启用Redis生成雪花WorkeId
        /// </summary>
        public bool RedisWorkeIdEnable { get; set; } = false;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DataBaseType { get; set; }

        /// <summary>
        /// 数据库连接字符串加密类型
        /// </summary>
        public string DbConnectStringCipherType { get; set; }

        /// <summary>
        /// 数据库连接串对象
        /// </summary>
        public Dictionary<string, string> ConnectionSettings { get; set; } = new Dictionary<string, string>();
    }
}