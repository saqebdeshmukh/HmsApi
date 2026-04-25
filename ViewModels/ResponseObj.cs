using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class ResponseObj<T> where T : class
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public bool IsSuccess { get; set; } = false;
    }
}
