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

    public async Task<string> GetAggregatedResults(string startDate, string endDate, string keyword, string sortDateBy, string sortNewsBy)
    {
        try
        {
            // Parse and validate date inputs
            var start = InputValidator.ParseAndValidateDate(startDate, nameof(startDate));
            var end = InputValidator.ParseAndValidateDate(endDate, nameof(endDate));

            // Validate other inputs
            InputValidator.ValidateInputs(keyword, sortDateBy, sortNewsBy);

            // Fetch data asynchronously from all services
            var apodTask = _apodService.FetchDataAsync(start, end);
            var newsTask = _newsService.FetchDataAsync(keyword, start, end, sortNewsBy);
            var weatherTask = _weatherService.FetchDataAsync(keyword, start, end);

            // Await all tasks simultaneously
            await Task.WhenAll(apodTask, newsTask, weatherTask);

            // Ensure data is not null and fetch the results
            var apodData = await apodTask ?? new List<APODModel>();
            var newsData = await newsTask ?? new NewsModel { Articles = new List<NewsModel.Article>() };
            var weatherData = await weatherTask ?? new List<WeatherModel>();

            // Aggregate results into a list of AggregatedResponse
            var aggregatedResults = AggregateResults(start, end, apodData, newsData, weatherData, sortDateBy);

            // Ensure that aggregatedResults is not null or empty, and always return valid JSON
            if (aggregatedResults == null || !aggregatedResults.Any())
            {
                aggregatedResults = new List<AggregatedResponse>(); // Empty list if no valid data
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
        catch (ArgumentException ex)
        {
            // Handle validation exceptions
            return $"Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred in GetAggregatedResults: {ex.Message}");
            throw new ServiceUnavailableException("An error occurred while processing your request. Please try again later.");
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