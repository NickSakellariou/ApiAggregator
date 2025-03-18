using System;
using System.Threading.Tasks;
using ApiAggregator.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiAggregator.Decorators
{
    public class AggregationServiceCachingDecorator : IAggregationService
    {
        private readonly IAggregationService _innerService;
        private readonly IDistributedCache _distributedCache;

        // You can configure these expiration times as needed
        private readonly TimeSpan _absoluteExpiration = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _slidingExpiration = TimeSpan.FromMinutes(2);

        public AggregationServiceCachingDecorator(
            IAggregationService innerService,
            IDistributedCache distributedCache)
        {
            _innerService = innerService;
            _distributedCache = distributedCache;
        }

        public async Task<string> GetAggregatedResults(
            string startDate,
            string endDate,
            string keyword,
            string sortDateBy,
            string sortNewsBy)
        {
            // Create a unique cache key based on query parameters
            var cacheKey = $"Aggregate_{startDate}_{endDate}_{keyword}_{sortDateBy}_{sortNewsBy}";

            // Try to retrieve from the distributed cache
            var cachedData = await _distributedCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return cachedData;
            }

            // If cache miss, call the underlying service to get fresh data
            var result = await _innerService.GetAggregatedResults(startDate, endDate, keyword, sortDateBy, sortNewsBy);

            // Define cache options
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _absoluteExpiration,
                SlidingExpiration = _slidingExpiration
            };

            // Cache the result as a string
            await _distributedCache.SetStringAsync(cacheKey, result, options);

            return result;
        }
    }
}
