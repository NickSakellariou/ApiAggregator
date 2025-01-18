using ApiAggregator.Interfaces;
using ApiAggregator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/[controller]")]
public class ApiAggregationController : ControllerBase
{
    private readonly IAggregationService _aggregationService;
    private readonly IMemoryCache _cache;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Limits to 1 concurrent request
    private readonly IStatisticsService _statisticsService;

    public ApiAggregationController(IAggregationService aggregationService, IMemoryCache cache, IStatisticsService statisticsService)
    {
        _aggregationService = aggregationService;
        _cache = cache;
        _statisticsService = statisticsService;
    }

    [HttpGet("aggregate")]
    public async Task<IActionResult> GetAggregatedData(
    [FromQuery] DateOnly startDate,
    [FromQuery] DateOnly endDate,
    [FromQuery] string keyword,
    [FromQuery] string sortDateBy,
    [FromQuery] string sortNewsBy)
    {
        // Generate a unique cache key based on query parameters
        string cacheKey = $"Aggregate_{startDate}_{endDate}_{keyword}_{sortDateBy}_{sortNewsBy}";

        // Measure time for cache retrieval
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Check cache for existing results
        if (_cache.TryGetValue(cacheKey, out var cachedResults))
        {
            stopwatch.Stop(); // Stop stopwatch after retrieving from cache

            // Log the time it took to retrieve cached results
            _statisticsService.RecordRequest("Cache", stopwatch.ElapsedMilliseconds);

            return Ok(cachedResults);
        }

        // If not cached, limit concurrent execution with a semaphore
        await _semaphore.WaitAsync();
        try
        {
            // Check cache again after acquiring semaphore (in case another thread cached it)
            if (_cache.TryGetValue(cacheKey, out cachedResults))
            {
                stopwatch.Stop(); // Stop stopwatch after retrieving from cache
                _statisticsService.RecordRequest("Cache", stopwatch.ElapsedMilliseconds);

                return Ok(cachedResults);
            }

            // Fetch data from the aggregation service
            var results = await _aggregationService.GetAggregatedResults(startDate, endDate, keyword, sortDateBy, sortNewsBy);

            stopwatch.Stop(); // Stop stopwatch after processing the request

            // Cache the results for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            _cache.Set(cacheKey, results, cacheOptions);

            return Ok(results);
        }
        finally
        {
            _semaphore.Release(); // Always release the semaphore to avoid deadlocks
        }
    }


    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        var statistics = _statisticsService.GetStatistics();
        return Ok(statistics);
    }
}
