using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BitsBlog.Application.Common;

public static class SortingExtensions
{
    public static IOrderedQueryable<TResult> OrderByProperty<TResult>(
        IQueryable<TResult> source,
        string? sortBy,
        string? sortOrder,
        string defaultProperty,
        bool strict = true)
    {
        var type = typeof(TResult);
        var propName = string.IsNullOrWhiteSpace(sortBy) ? defaultProperty : sortBy!;

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var prop = props.FirstOrDefault(p => string.Equals(p.Name, propName, StringComparison.OrdinalIgnoreCase));

        if (prop is null)
        {
            if (strict)
                throw new BitsBlog.Application.Common.Exceptions.InvalidSortByException(sortBy, props.Select(p => p.Name));
            // fallback to default
            prop = type.GetProperty(defaultProperty, BindingFlags.Public | BindingFlags.Instance)
                   ?? throw new ArgumentException($"Default sort property '{defaultProperty}' not found on {type.Name}.");
        }

        var param = Expression.Parameter(type, "x");
        var body = Expression.Property(param, prop);
        var lambda = Expression.Lambda(body, param);

        var methodName = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
            ? "OrderByDescending"
            : "OrderBy";

        var method = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(type, prop.PropertyType);

        var ordered = (IOrderedQueryable<TResult>)method.Invoke(null, new object[] { source, lambda })!;
        return ordered;
    }
}