using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Pursue.Extension.DataBase
{
    /// <summary>
    /// 分页查询基础对象
    /// </summary>
    public abstract class PageQuery
    {
        /// <summary>
        /// 当前页数
        /// </summary>
        [property: Required]
        [property: DefaultValue(1)]
        [property: Description("当前页数")]
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// 每页条数
        /// </summary>
        [property: Required]
        [property: DefaultValue(10)]
        [property: Description("每页条数")]
        public int PageSize { get; set; } = 10;
    }
}