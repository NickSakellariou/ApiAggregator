using ApiAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAggregator.Tests.Models
{
    public class AggregatedResponseWrapper
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<AggregatedResponse> Data { get; set; }
    }
}
