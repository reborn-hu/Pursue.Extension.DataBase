using System.ComponentModel;

namespace Pursue.Extension.DataBase
{
    /// <summary>
    /// 连接字符串加密类型
    /// </summary>
    public enum DbCipherType
    {
        [Description("plaintext")]
        plaintext,

        [Description("ciphertext")]
        ciphertext
    }
}
