#nullable enable
using System.Collections.Generic;

namespace Crey.Data
{
    public class CursorListResult<T>
        where T : notnull
    {
        public List<T> Items { get; set; }
        public string? Cursor { get; set; }

        public CursorListResult(List<T> items, string? cursor)
        {
            Items = items;
            Cursor = cursor;
        }
    }
}