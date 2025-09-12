using BitsBlog.Application.DTO.Common;
using BitsBlog.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitsBlog.Application.Common;

public static class RepositoryPagingExtensions
{
    public static async Task<PagedResult<TResult>> PagedAsync<T, TResult>(
        this IRepository<T> repository,
        IQueryable<TResult> projected,
        string? sortBy,
        string? sortOrder,
        string defaultSortProperty,
        int page,
        int pageSize,
        bool strict = true)
        where T : class
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var skip = (page - 1) * pageSize;
        var ordered = SortingExtensions.OrderByProperty(projected, sortBy, sortOrder, defaultSortProperty, strict);
        var items = await repository.ToListAsync(ordered.Skip(skip).Take(pageSize));
        var total = await repository.CountAsync(projected);

        return new PagedResult<TResult>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}

