using System.Collections.Generic;

namespace BitsBlog.Web.Models
{
    public class PaginationModel
    {
        public string Action { get; set; } = "Index";
        public string? Controller { get; set; }
        public string? Area { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int Total { get; set; }
        public IDictionary<string, object?> ExtraRouteValues { get; set; } = new Dictionary<string, object?>();

        // UI options
        public bool ShowPageSize { get; set; } = true;
        public int[] PageSizeOptions { get; set; } = new[] { 10, 20, 50, 100 };
    }
}
