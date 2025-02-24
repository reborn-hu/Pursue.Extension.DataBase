using System;
using System.ComponentModel;

namespace Pursue.Extension.DataBase
{
    public abstract class Dto
    {
        /// <summary>
        /// 自增Id
        /// </summary>
        [property: Description("数据唯一编码")]
        public string Id { get; set; }

        /// <summary>
        /// 有效标识  1:有效 0:无效
        /// </summary>
        [property: Description("有效标识  1:有效 0:无效")]
        public bool Active { get; set; } = false;

        /// <summary>
        /// 删除标识  1:删除 0:未删除
        /// </summary>
        [property: Description("删除标识  1:删除 0:未删除")]
        public bool Del { get; set; } = false;

        /// <summary>
        /// 创建人
        /// </summary>
        [property: Description("创建人")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [property: Description("创建时间")]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        [property: Description("操作人")]
        public string ModifyUser { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        [property: Description("操作时间")]
        public DateTime? ModifyDate { get; set; }
    }
}