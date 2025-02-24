namespace Pursue.Extension.DataBase
{
    public sealed class Tenant : ITenant
    {
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 租户Id
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 用户上下文信息
        /// </summary>
        public UserContext UserContext { get; set; }
    }
}
