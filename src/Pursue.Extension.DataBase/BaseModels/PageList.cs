using System.Collections.Generic;
using System.ComponentModel;

namespace Pursue.Extension.DataBase
{
    /// <summary>
    /// 分页结果对象
    /// </summary>
    /// <typeparam name="Tentity"></typeparam>
    public sealed class PageList<Tentity> where Tentity : class
    {
        /// <summary>
        /// 总条数
        /// </summary>
        [property: Description("总条数")]
        public int Count { get; private set; }

        /// <summary>
        /// 数据集合
        /// </summary>
        [property: Description("数据集合")]
        public IEnumerable<Tentity> Items { get; private set; }


        public PageList()
        {
            Count = 0;
            Items = new List<Tentity>();
        }


        public PageList(int count, IEnumerable<Tentity> items)
        {
            Count = count;
            Items = items;
        }
    }
}