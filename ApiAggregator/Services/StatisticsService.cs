using ApiAggregator.Interfaces;
using ApiAggregator.Models;

namespace ApiAggregator.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly List<ApiStatistics> _stats = new();

        public void RecordRequest(string apiName, long responseTime)
        {
            var apiStat = _stats.FirstOrDefault(s => s.ApiName == apiName);
            if (apiStat == null)
            {
                apiStat = new ApiStatistics { ApiName = apiName };
                _stats.Add(apiStat);
            }
            apiStat.TotalRequests++;
            apiStat.ResponseTimes.Add(responseTime);

            // Log the recorded statistics
            Console.WriteLine($"Recorded request for {apiName} with response time: {responseTime}ms");
        }

        public List<object> GetStatistics()
        {
            return _stats.Select(stat => new
            {
                ApiName = stat.ApiName,
                TotalRequests = stat.TotalRequests,
                FastRequests = stat.ResponseTimes.Count(rt => rt < 100),
                AverageRequests = stat.ResponseTimes.Count(rt => rt >= 100 && rt <= 200),
                SlowRequests = stat.ResponseTimes.Count(rt => rt > 200),
                AverageResponseTime = stat.ResponseTimes.Any() ? stat.ResponseTimes.Average() : 0
            }).ToList<object>();
        }
    }
}
