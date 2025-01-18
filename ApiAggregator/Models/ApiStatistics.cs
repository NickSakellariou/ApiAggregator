namespace ApiAggregator.Models
{
    public class ApiStatistics
    {
        public string ApiName { get; set; }
        public int TotalRequests { get; set; }
        public List<long> ResponseTimes { get; set; } = new List<long>();
    }
}
