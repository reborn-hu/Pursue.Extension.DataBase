namespace Pursue.Extension.DataBase
{
    public interface ITenant
    {
        /// <summary>
        /// Token
        /// </summary>
        string Token { get; set; }

        /// <summary>
        /// 租户Id
        /// </summary>
        string TenantId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        string UserId { get; set; }

        /// <summary>
        /// 用户上下文信息
        /// </summary>
        UserContext UserContext { get; set; }
    }
}