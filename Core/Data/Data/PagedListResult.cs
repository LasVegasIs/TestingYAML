#nullable enable
using System.Collections.Generic;

namespace Crey.Data
{
    public class PagedListResult<T>
    {
        public List<T> Items { get; set; } = null!;
        public string? ContinuationToken { get; set; }
    }
}