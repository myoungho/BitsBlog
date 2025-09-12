using System.Collections.Generic;

namespace BitsBlog.Application.DTOs.Common;

public record class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Total { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
}

