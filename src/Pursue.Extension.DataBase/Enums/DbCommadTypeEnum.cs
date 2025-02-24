using System.ComponentModel;

namespace Pursue.Extension.DataBase
{
    /// <summary>
    /// 数据库执行类型
    /// </summary>
    public enum DbCommadType
    {
        /// <summary>
        /// 读库
        /// </summary>
        [Description("Read")]
        Read,

        /// <summary>
        /// 写库
        /// </summary>
        [Description("Write")]
        Write
    }
}
