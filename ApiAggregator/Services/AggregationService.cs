using ApiAggregator.Exceptions;
using ApiAggregator.Interfaces;
using ApiAggregator.Models;
using ApiAggregator.Services;
using ApiAggregator.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static ApiAggregator.Models.NewsModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class AggregationService : IAggregationService
{
    private readonly IAPODService _apodService;
    private readonly INewsService _newsService;
    private readonly IWeatherService _weatherService;

    public AggregationService(IAPODService apodService, INewsService newsService, IWeatherService weatherService)
    {
        _apodService = apodService;
        _newsService = newsService;
        _weatherService = weatherService;
    }

    public async Task<string> GetAggregatedResults(string startDate, string endDate, string keyword, string sortDateBy, string sortNewsBy)
    {
        List<string> errors = new List<string>();

        try
        {
            // Parse and validate dates
            var start = InputValidator.ParseAndValidateDate(startDate, nameof(startDate));
            var end = InputValidator.ParseAndValidateDate(endDate, nameof(endDate));

            InputValidator.ValidateDateRange(start, end);

            // Validate other inputs
            InputValidator.ValidateInputs(keyword, sortDateBy, sortNewsBy);

            // Start the asynchronous operations directly.
            var apodTask = _apodService.FetchDataAsync(start, end)
                .ContinueWith(t => t.IsFaulted ? (List<APODModel>)null : t.Result);
            var newsTask = _newsService.FetchDataAsync(keyword, start, end, sortNewsBy)
                .ContinueWith(t => t.IsFaulted ? new NewsModel { Articles = new List<NewsModel.Article>() } : t.Result);
            var weatherTask = _weatherService.FetchDataAsync(keyword, start, end)
                .ContinueWith(t => t.IsFaulted ? (List<WeatherModel>)null : t.Result);

            // Await them all concurrently.
            await Task.WhenAll(apodTask, newsTask, weatherTask);

            // Now aggregate the results, handling potential nulls.
            var aggregatedResults = AggregateResults(
                start,
                end,
                apodTask.Result ?? new List<APODModel>(),
                newsTask.Result ?? new NewsModel { Articles = new List<NewsModel.Article>() },
                weatherTask.Result ?? new List<WeatherModel>(),
                sortDateBy
            );

            return JsonSerializer.Serialize(new UnifiedResponse<List<AggregatedResponse>>
            {
                Status = errors.Count == 0 ? "success" : (errors.Count < 3 ? "partial_failure" : "failure"),
                Message = errors.Count == 0
                ? "Data aggregation successful."
                : $"Some data sources are unavailable: {string.Join(", ", errors)}.",
                Data = aggregatedResults
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private List<AggregatedResponse> AggregateResults(DateOnly start, DateOnly end, List<APODModel> apodData, NewsModel newsData, List<WeatherModel> weatherData, string sortDateBy)
    {
        var aggregatedResults = new List<AggregatedResponse>();

        // Iterate through the date range
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            // Find matching data for the current date
            var weatherForDate = weatherData.FirstOrDefault(weather => weather.Date == date);
            var apodForDate = apodData.FirstOrDefault(apod => apod.Date == date.ToString("yyyy-MM-dd"));
            var newsForDate = newsData.Articles.Where(article => article.PublishedAt.Date == date.ToDateTime(TimeOnly.MinValue).Date).ToList();

            // Create the aggregated response for the current date
            var aggregatedResponse = new AggregatedResponse
            {
                Date = date,
                Weather = weatherForDate?.Temperature,
                AstronomyPictureOfTheDay = apodForDate,
                News = new NewsModel
                {
                    Articles = newsForDate
                }
            };

            aggregatedResults.Add(aggregatedResponse);
        }

        // Sort aggregated results if necessary
        if (!string.IsNullOrWhiteSpace(sortDateBy))
        {
            aggregatedResults = sortDateBy.ToLower() switch
            {
                "asc" => aggregatedResults.OrderBy(result => result.Date).ToList(),
                "desc" => aggregatedResults.OrderByDescending(result => result.Date).ToList(),
                _ => aggregatedResults
            };
        }

        return aggregatedResults;
    }
}