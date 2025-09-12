using System;
using System.Linq;
using System.Net.Http;

namespace BitsBlog.Web.Services
{
    public static class PagingUtils
    {
        public static (int page, int pageSize) Normalize(int page, int pageSize, int defaultPageSize = 10, int maxPageSize = 100)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = defaultPageSize;
            if (pageSize > maxPageSize) pageSize = maxPageSize;
            return (page, pageSize);
        }

        public static int ParseTotalCount(HttpResponseMessage res)
        {
            if (res.Headers.TryGetValues("X-Total-Count", out var vals))
            {
                var s = vals.FirstOrDefault();
                if (int.TryParse(s, out var total)) return total;
            }
            return 0;
        }

        public static int TotalPages(int total, int pageSize)
        {
            if (pageSize <= 0) return 0;
            return (int)Math.Ceiling((double)total / pageSize);
        }
    }
}

