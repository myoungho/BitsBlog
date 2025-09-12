using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitsBlog.Application.Common.Exceptions;

public class InvalidSortByException : ArgumentException
{
    public IReadOnlyList<string> AllowedFields { get; }

    public InvalidSortByException(string? sortBy, IEnumerable<string> allowed)
        : base($"Invalid SortBy: '{sortBy}'. Allowed fields: {string.Join(", ", allowed)}")
    {
        AllowedFields = allowed.ToArray();
    }
}
