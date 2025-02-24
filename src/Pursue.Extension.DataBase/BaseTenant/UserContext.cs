namespace Pursue.Extension.DataBase
{
    public sealed class UserContext
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 所属组织
        /// </summary>
        public object Orgs { get; set; }

        /// <summary>
        /// 所属角色
        /// </summary>
        public object Roles { get; set; }

        /// <summary>
        /// 菜单
        /// </summary>
        public object Menus { get; set; }

        /// <summary>
        /// 权限位
        /// </summary>
        public object Permissions { get; set; }

        /// <summary>
        /// 是否绑定MFA
        /// </summary>
        public bool MfaBind { get; set; }

        /// <summary>
        /// 是否开启MFA验证
        /// </summary>
        public bool MfaVerifyEnable { get; set; }

        /// <summary>
        /// MFA秘钥
        /// </summary>
        public string MfaSecret { get; set; }

        /// <summary>
        /// 受众
        /// </summary>
        public string Audience { get; set; }
    }
}
