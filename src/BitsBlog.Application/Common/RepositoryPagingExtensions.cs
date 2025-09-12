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
    public static async Task<PagedResult<TResult>> PagedAsync<T, TResult>(
        this IRepository<T> repository,
        Func<IQueryable<T>, IQueryable<TResult>> selector,
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

        var items = await repository.QueryAsync(q =>
        {
            var projected = selector(q);
            var ordered = SortingExtensions.OrderByProperty(projected, sortBy, sortOrder, defaultSortProperty, strict);
            return ordered.Skip(skip).Take(pageSize);
        });

        var total = await repository.CountAsync(selector);

        return new PagedResult<TResult>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public static async Task<PagedResult<TResult>> PagedAsync<T, TResult>(
        this IRepository<T> repository,
        Func<IQueryable<T>, IQueryable<TResult>> selector,
        Func<IQueryable<TResult>, IOrderedQueryable<TResult>> order,
        int page,
        int pageSize)
        where T : class
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var skip = (page - 1) * pageSize;

        var items = await repository.QueryAsync(q => order(selector(q)).Skip(skip).Take(pageSize));
        var total = await repository.CountAsync(selector);

        return new PagedResult<TResult>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public static async Task<PagedResult<TResult>> PagedAsync<T, TResult>(
        this IRepository<T> repository,
        Func<IQueryable<T>, IQueryable<TResult>> queryBuilder,
        int page,
        int pageSize)
        where T : class
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var skip = (page - 1) * pageSize;

        var items = await repository.QueryAsync(q =>
            queryBuilder(q).Skip(skip).Take(pageSize));

        var total = await repository.CountAsync(q => queryBuilder(q));

        return new PagedResult<TResult>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}

