using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Pursue.Extension.DataBase
{
    public static class ORM
    {
        /// <summary>
        /// 初始化连接
        /// </summary>
        public static SqlSugarScope SqlSugar = new SqlSugarScope(new ConnectionConfig()
        {
            ConfigId = DbBusinessType.Base.GetDescription(),
            DbType = ConnectionOptions.DbType,
            ConnectionString = ConnectionOptions.BaseConnectString,
            IsAutoCloseConnection = true,
            AopEvents = new AopEvents
            {
                OnLogExecuted = (str, param) =>
                {
                    var paramStr = param.ToList().Select(o => $"{o.ParameterName}:{o.Value}");

                    Log.DeBug("Sql:{},Param:{}", str, paramStr == null && !paramStr.Any() ? string.Empty : string.Join(",", paramStr));
                },
                OnError = (err) =>
                {
                    Log.Error(err);
                }
            }
        });

        /// <summary>
        /// 连接切换
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="businessType"></param>
        /// <param name="connectString"></param>
        /// <returns></returns>
        internal static ISqlSugarClient SwitchDbContext(string tenantId, DbBusinessType businessType, string connectString)
        {
            var configId = $"{tenantId}.{businessType}.{connectString}".ToMd5();

            if (!SqlSugar.IsAnyConnection(configId))
            {
                SqlSugar.AddConnection(new ConnectionConfig()
                {
                    ConfigId = configId,
                    ConnectionString = connectString,
                    DbType = ConnectionOptions.DbType,
                    IsAutoCloseConnection = true,
                    AopEvents = new AopEvents
                    {
                        OnLogExecuted = (str, param) =>
                        {
                            var paramStr = param.ToList().Select(o => $"{o.ParameterName}:{o.Value}");
                            Log.DeBug("Sql:{},Param:{}", str, paramStr == null && !paramStr.Any() ? string.Empty : string.Join(",", paramStr));
                        },
                        OnError = (err) =>
                        {
                            Log.Error(err);
                        }
                    }
                });
            }

            return SqlSugar.GetConnectionScope(configId);
        }

        public static class Convert
        {
            public static List<SugarParameter> ToParameter<TParam>(string sql, TParam obj)
            {
                var parameters = new List<SugarParameter>();
                var properties = typeof(TParam).GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (var propertyInfo in properties)
                {
                    if (sql.ToLower().IndexOf($"@{propertyInfo.Name.ToLower()}") > -1)
                    {
                        var dbType = propertyInfo.PropertyType.GetDataType();
                        var value = propertyInfo.GetValue(obj);
                        var columnName = propertyInfo.Name;

                        var attributes = propertyInfo.GetCustomAttribute(typeof(SugarColumn));
                        if (attributes != null)
                        {
                            var columnAttributes = attributes as SugarColumn;
                            var size = columnAttributes.Length;
                            if (propertyInfo.PropertyType == StringType)
                            {
                                if (Enum.TryParse(columnAttributes.SqlParameterDbType.ToString(), out System.Data.DbType type))
                                {
                                    dbType = type;
                                }
                            }
                            parameters.Add(new SugarParameter(columnName, value, dbType, ParameterDirection.Input, size));
                        }
                        else
                        {
                            parameters.Add(new SugarParameter(columnName, value, dbType));
                        }
                    }
                }
                return parameters;
            }
        }

        #region 类型转换

        internal static Type StringType = typeof(string);
        internal static Type CharType = typeof(char);

        internal static Type ByteArrayType = typeof(byte[]);
        internal static Type GuidType = typeof(Guid);

        internal static Type BoolType = typeof(bool);
        internal static Type BoolTypeNull = typeof(bool?);

        internal static Type ByteType = typeof(byte);
        internal static Type ByteTypeNull = typeof(byte?);

        internal static Type SByteType = typeof(sbyte);
        internal static Type SByteTypeNull = typeof(sbyte?);

        internal static Type DateTimeType = typeof(DateTime);
        internal static Type DateTimeTypeNull = typeof(DateTime?);

        internal static Type DecimalType = typeof(decimal);
        internal static Type DecimalTypeNull = typeof(decimal?);
        internal static Type DoubleType = typeof(double);
        internal static Type DoubleTypeNull = typeof(double?);
        internal static Type ShortType = typeof(short);
        internal static Type ShortTypeNull = typeof(short?);
        internal static Type IntType = typeof(int);
        internal static Type IntTypeNull = typeof(int?);
        internal static Type LongType = typeof(long);
        internal static Type LongTypeNull = typeof(long?);
        internal static Type FloatType = typeof(float);
        internal static Type FloatTypeNull = typeof(float?);

        private static System.Data.DbType GetDataType(this Type type)
        {
            if (type == StringType)
            {
                return System.Data.DbType.String;
            }
            else if (type == CharType)
            {
                return System.Data.DbType.AnsiStringFixedLength;
            }
            else if (type == ByteArrayType)
            {
                return System.Data.DbType.Binary;
            }
            else if (type == GuidType)
            {
                return System.Data.DbType.Guid;
            }
            else if (type == BoolType || type == BoolTypeNull)
            {
                return System.Data.DbType.Boolean;
            }
            else if (type == ByteType || type == ByteTypeNull)
            {
                return System.Data.DbType.Byte;
            }
            else if (type == SByteType || type == SByteTypeNull)
            {
                return System.Data.DbType.SByte;
            }
            else if (type == DateTimeType || type == DateTimeTypeNull)
            {
                return System.Data.DbType.DateTime;
            }
            else if (type == DecimalType || type == DecimalTypeNull)
            {
                return System.Data.DbType.Decimal;
            }
            else if (type == DoubleType || type == DoubleTypeNull)
            {
                return System.Data.DbType.Double;
            }
            else if (type == ShortType || type == ShortTypeNull)
            {
                return System.Data.DbType.Int16;
            }
            else if (type == IntType || type == IntTypeNull)
            {
                return System.Data.DbType.Int32;
            }
            else if (type == LongType || type == LongTypeNull)
            {
                return System.Data.DbType.Int64;
            }
            else if (type == FloatType || type == FloatTypeNull)
            {
                return System.Data.DbType.Single;
            }
            else
            {
                return System.Data.DbType.Object;
            }
        }

        #endregion

        #region 连接串加密解密

        private const string CipherKey = "tOKuM0kHv3lR2Xcg4mYVfT7EwiUhFno9";

        public static string DecodeConnectString(this string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "连接字符串不可为空！");
            switch (ConnectionOptions.DbCipherType)
            {
                // 明文
                case DbCipherType.plaintext:
                    return connectionString;
                // 普通加密
                case DbCipherType.ciphertext:
                    var plaintext = connectionString.DecryptConnectString();
                    if (!string.IsNullOrWhiteSpace(plaintext))
                    {
                        return plaintext;
                    }
                    throw new ArgumentNullException(nameof(connectionString), "DbConnectCipherType.ciphertext 解密失败！");
                // 异常
                default:
                    throw new ArgumentNullException(nameof(connectionString), "请设置DbConnectStringCipherType配置指定连接串加密模式！");
            }
        }

        public static string DecryptConnectString(this string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        var charArray = connectionString.ToCharArray();
                        var sb = new StringBuilder();

                        for (var i = 0; i < charArray.Length; i++)
                        {
                            sb.Append(charArray[i]);

                            if (i % 2 == 1)
                            {
                                ms.WriteByte(System.Convert.ToByte(sb.ToString(), 16));
                                sb.Remove(0, sb.Length);
                            }
                        }

                        var data = ms.ToArray();
                        using (var aes = Aes.Create())
                        {
                            using (var cache = new MemoryStream())
                            {
                                var key = GetCryptographyKey();
                                using (var stream = new CryptoStream(cache, aes.CreateDecryptor(key, GetIVByKey(key)), CryptoStreamMode.Write))
                                {
                                    stream.Write(data, 0, data.Length);
                                    stream.FlushFinalBlock();

                                    return new string(cache.ToArray().Select(System.Convert.ToChar).ToArray());
                                }
                            }
                        }
                    }
                }
                catch
                {
                    return connectionString;
                }
            }
            return connectionString;
        }

        public static string EncryptConnectString(this string connectionString)
        {
            var result = string.Empty;
            if (string.IsNullOrWhiteSpace(connectionString))
                return result;
            try
            {
                byte[] key = GetCryptographyKey();
                byte[] data = connectionString.ToCharArray().Select(System.Convert.ToByte).ToArray();

                using (var aes = Aes.Create())
                {
                    using (var cache = new MemoryStream())
                    {
                        using (var stream = new CryptoStream(cache, aes.CreateEncryptor(key, GetIVByKey(key)), CryptoStreamMode.Write))
                        {
                            stream.Write(data, 0, data.Length);
                            stream.FlushFinalBlock();

                            return cache.ToArray().BuildToken();
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private static byte[] GetCryptographyKey()
        {
            return SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(CipherKey));
        }

        private static string BuildToken(this byte[] data)
        {
            var sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }

        private static byte[] GetIVByKey(byte[] key)
        {
            byte[] iv = new byte[key.Length / 2];

            Array.Copy(key, iv, iv.Length);

            return iv;
        }

        private static string ToMd5(this string input)
        {
            using var md5 = MD5.Create();
            var strResult = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "");
            return strResult;
        }

        #endregion
    }
}