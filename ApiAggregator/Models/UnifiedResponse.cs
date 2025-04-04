﻿namespace ApiAggregator.Models
{
    public class UnifiedResponse<T>
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
