namespace Snet.Iot.Debug.model
{
    /// <summary>
    /// 分页结果类，用于封装分页后的数据及相关分页信息。
    /// </summary>
    /// <typeparam name="T">数据项类型</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// 当前页的数据项集合。
        /// </summary>
        public List<T> Items { get; set; } = [];

        /// <summary>
        /// 原始集合的总数据条数。
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页的索引（从 0 开始）。
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页的最大条目数。
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 当前页实际加载的数据条数（可能小于 PageSize）。
        /// </summary>
        public int CurrentCount => Items.Count;

        /// <summary>
        /// 是否为最后一页。
        /// 如果 当前已加载的数据 >= 总数据量，则认为是最后一页。
        /// </summary>
        public bool IsLastPage => PageIndex * PageSize + Items.Count >= TotalCount;
    }
}
