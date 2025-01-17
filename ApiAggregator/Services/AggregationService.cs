using ApiAggregator.Exceptions;
using ApiAggregator.Interfaces;
using ApiAggregator.Models;
using ApiAggregator.Services;
using ApiAggregator.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static ApiAggregator.Models.NewsModel;

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

    public async Task<string> GetAggregatedResults(DateOnly startDate, DateOnly endDate, string keyword, string sortDateBy, string sortNewsBy)
    {
        try
        {
            // Validate inputs
            InputValidator.ValidateDateRange(startDate, endDate);

            // Fetch data asynchronously from all services
            var apodTask = _apodService.FetchDataAsync(startDate, endDate);
            var newsTask = _newsService.FetchDataAsync(keyword, startDate, endDate, sortNewsBy);
            var weatherTask = _weatherService.FetchDataAsync(keyword, startDate, endDate);

            // Await all tasks simultaneously
            await Task.WhenAll(apodTask, newsTask, weatherTask);

            var apodData = await apodTask ?? new List<APODModel>();
            var newsData = await newsTask ?? new NewsModel { Articles = new List<NewsModel.Article>() };
            var weatherData = await weatherTask ?? new List<WeatherModel>();

            // Aggregate results into a list of AggregatedResponse
            var aggregatedResults = new List<AggregatedResponse>();

            // Iterate through the date range
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var formattedDate = date.ToString("yyyy-MM-dd");

                // Find matching data for the current date
                var weatherForDate = weatherData?.FirstOrDefault(weather => weather.Date.ToString("yyyy-MM-dd") == formattedDate);
                var apodForDate = apodData.FirstOrDefault(apod => apod.Date == formattedDate);
                var newsForDate = newsData.Articles.Where(article => article.PublishedAt.ToString("yyyy-MM-dd") == formattedDate).ToList();

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

            // Sort aggregated results based on sortDateBy
            if (!string.IsNullOrWhiteSpace(sortDateBy))
            {
                aggregatedResults = sortDateBy.ToLower() switch
                {
                    "asc" => aggregatedResults.OrderBy(result => result.Date).ToList(),
                    "desc" => aggregatedResults.OrderByDescending(result => result.Date).ToList(),
                    _ => aggregatedResults // Default: keep original order
                };
            }

            // Serialize to JSON for output
            var finalJson = JsonSerializer.Serialize(aggregatedResults, new JsonSerializerOptions
            {
                WriteIndented = true // Optional: Makes the JSON output pretty-printed
            });

            return finalJson;
        }
        catch (ApiAggregator.Exceptions.ValidationException ex)
        {
            Console.WriteLine($"Validation error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred in GetAggregatedResults: {ex.Message}");
            throw new ServiceUnavailableException("An error occurred while processing your request. Please try again later.");
        }
    }
}