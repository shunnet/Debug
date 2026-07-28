using Opc.Ua;
using Snet.Iot.Debug.model;

namespace Snet.Iot.Debug.handler
{
    /// <summary>
    /// ReferenceDescriptionCollection 的分页扩展方法
    /// </summary>
    public static class PageHandler
    {
        /// <summary>
        /// 对 ReferenceDescriptionCollection 进行分页处理，返回包含分页信息的结果对象。
        /// </summary>
        /// <param name="source">原始 ReferenceDescriptionCollection 集合</param>
        /// <param name="pageIndex">页索引，从 0 开始</param>
        /// <param name="pageSize">每页数据条数，必须大于 0</param>
        /// <returns>PagedResult 对象，包含当前页数据、总数、页码等信息</returns>
        public static PagedResult<ReferenceDescription> ToPagedResult(this ReferenceDescriptionCollection source, int pageIndex, int pageSize = 25)
        {
            // 参数校验
            if (source == null)
                throw new ArgumentNullException(nameof(source), "源集合不能为空。");
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "页索引不能小于 0。");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "每页大小必须大于 0。");

            // 计算需要跳过的项数
            int skip = pageIndex * pageSize;

            // 如果跳过的数量超过集合总数，返回空页
            var items = skip >= source.Count
                ? new List<ReferenceDescription>()
                : source.Skip(skip).Take(pageSize).ToList();
            // 构造分页结果对象
            return new PagedResult<ReferenceDescription>
            {
                Items = items,
                TotalCount = source.Count,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

    }
}
