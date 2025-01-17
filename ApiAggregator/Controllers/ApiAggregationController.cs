using ApiAggregator.Interfaces;
using ApiAggregator.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ApiAggregationController : ControllerBase
{
    private readonly IAggregationService _aggregationService;

    public ApiAggregationController(IAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    [HttpGet("aggregate")]
    public async Task<IActionResult> GetAggregatedData(
    [FromQuery] DateOnly startDate,
    [FromQuery] DateOnly endDate,
    [FromQuery] string keyword,
    [FromQuery] string sortDateBy,
    [FromQuery] string sortNewsBy)
    {
        var results = await _aggregationService.GetAggregatedResults(startDate, endDate, keyword, sortDateBy, sortNewsBy);

        return Ok(results);
    }
}
