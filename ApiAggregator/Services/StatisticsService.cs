using ApiAggregator.Interfaces;
using ApiAggregator.Models;
using System.Collections.Concurrent;

namespace ApiAggregator.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ConcurrentDictionary<string, ApiStatistics> _stats = new();

        public void RecordRequest(string apiName, long responseTime)
        {
            // Retrieve existing or create a new ApiStatistics record for the given apiName in a thread-safe manner.
            var apiStat = _stats.GetOrAdd(apiName, key => new ApiStatistics { ApiName = key });

            // Lock on the specific record to update it safely.
            lock (apiStat)
            {
                apiStat.TotalRequests++;
                apiStat.ResponseTimes.Add(responseTime);
            }

            Console.WriteLine($"Recorded request for {apiName} with response time: {responseTime}ms");
        }

        public List<object> GetStatistics()
        {
            return _stats.Values.Select(stat => new
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
