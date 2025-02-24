using System.ComponentModel;

namespace Pursue.Extension.DataBase
{
    /// <summary>
    /// 数据库业务类型
    /// </summary>
    public enum DbBusinessType
    {
        /// <summary>
        /// 基础库
        /// </summary>
        [Description("BASE")]
        Base = 0,

        /// <summary>
        /// 业务库
        /// </summary>
        [Description("BIZ")]
        Biz = 2,

        /// <summary>
        /// 日志库
        /// </summary>
        [Description("LOGGER")]
        Logger = 3
    }
}
