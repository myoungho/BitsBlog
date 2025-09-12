using System.Collections.Generic;

namespace BitsBlog.Web.Models
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int Total { get; }

        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int total)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            Total = total;
        }
    }
}

