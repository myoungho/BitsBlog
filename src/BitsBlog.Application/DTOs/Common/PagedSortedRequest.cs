namespace BitsBlog.Application.DTOs.Common;

public class PagedSortedRequest : PagedRequest
{
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

