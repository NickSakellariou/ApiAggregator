using ApiAggregator.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ApiAggregationController : ControllerBase
{
    private readonly IAggregationService _aggregationService;
    private readonly IStatisticsService _statisticsService;

    public ApiAggregationController(
        IAggregationService aggregationService,
        IStatisticsService statisticsService)
    {
        _aggregationService = aggregationService;
        _statisticsService = statisticsService;
    }

    [HttpGet("aggregate")]
    public async Task<IActionResult> GetAggregatedData(
        [FromQuery] string startDate,
        [FromQuery] string endDate,
        [FromQuery] string keyword,
        [FromQuery] string sortDateBy,
        [FromQuery] string sortNewsBy)
    {
        // Call the service; caching is handled in the decorator.
        var results = await _aggregationService.GetAggregatedResults(startDate, endDate, keyword, sortDateBy, sortNewsBy);
        return Ok(results);
    }

    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        var statistics = _statisticsService.GetStatistics();
        return Ok(statistics);
    }
}
