using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class PaginationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; } = "asc";
        public Dictionary<string, string>? Filters { get; set; }
    }
    public class PagedResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Data { get; set; } = new();
    }
}
