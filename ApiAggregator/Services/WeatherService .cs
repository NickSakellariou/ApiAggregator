using ApiAggregator.Models;
using ApiAggregator.Interfaces;
using ApiAggregator.Utilities;
using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;

namespace ApiAggregator.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _geoUrl;
        private readonly string _baseUrl;
        private readonly IStatisticsService _statisticsService; // Injected statistics service

        public WeatherService(HttpClient httpClient, IConfiguration configuration, IStatisticsService statisticsService)
        {
            _httpClient = httpClient;
            _geoUrl = configuration["ExternalApis:OpenWeatherMap:GeoUrl"];
            _baseUrl = configuration["ExternalApis:OpenWeatherMap:BaseUrl"];
            _apiKey = configuration["ExternalApis:OpenWeatherMap:ApiKey"];
            _statisticsService = statisticsService;
        }

        public async Task<List<WeatherModel>> FetchDataAsync(string city, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                // Step 1: Get Latitude and Longitude for the city
                var geoLocation = await GetLocationAsync(city);
                if (geoLocation == null)
                {
                    return new List<WeatherModel>(); // Return an empty list if no location is found
                }

                // Step 2: Fetch Weather Data for each date in the range
                return await FetchWeatherDataAsync(geoLocation.Lat, geoLocation.Lon, startDate, endDate);
            }
            catch (Exception ex)
            {
                // Log the error (e.g., using a logging library)
                Console.WriteLine($"Error fetching weather data: {ex.Message}");
                // Return an empty list or consider implementing a fallback mechanism
                return new List<WeatherModel>();
            }
        }

        public async Task<GeoLocation> GetLocationAsync(string city)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start stopwatch to measure response time
            try
            {
                var url = $"{_geoUrl}?q={city}";
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                // Use the Polly retry policy
                var response = await PollyPolicies.GetRetryPolicy().ExecuteAsync(() => _httpClient.GetAsync(url));

                stopwatch.Stop(); // Stop stopwatch after the response is received

                // Record the response time
                _statisticsService.RecordRequest("Weather - Location", stopwatch.ElapsedMilliseconds);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Log the error for monitoring
                    Console.WriteLine($"Error fetching data: {response.StatusCode}, {errorContent}");

                    // Decide based on status code
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null; // API returned no data, but it's not critical
                    }

                    throw new HttpRequestException($"API call failed: {response.StatusCode}, Details: {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var locations = JsonSerializer.Deserialize<List<GeoLocation>>(json);

                return locations?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in GetLocationAsync: {ex.Message}");

                stopwatch.Stop(); // Stop stopwatch after the response is received

                // Record the response time
                _statisticsService.RecordRequest("Weather - Location", stopwatch.ElapsedMilliseconds);
                return null;
            }
        }

        public async Task<List<WeatherModel>> FetchWeatherDataAsync(double lat, double lon, DateOnly startDate, DateOnly endDate)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start stopwatch to measure response time
            var weatherData = new List<WeatherModel>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                try
                {
                    var weatherUrl = $"{_baseUrl}?lat={lat}&lon={lon}&date={date:yyyy-MM-dd}&units=metric";

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                    // Use the Polly retry policy
                    var response = await PollyPolicies.GetRetryPolicy().ExecuteAsync(() => _httpClient.GetAsync(weatherUrl));

                    stopwatch.Stop(); // Stop stopwatch after the response is received

                    // Record the response time for each weather request
                    _statisticsService.RecordRequest("Weather", stopwatch.ElapsedMilliseconds);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Weather API call failed for {date:yyyy-MM-dd} with status code: {response.StatusCode}");
                        continue; // Skip to the next date
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var weather = JsonSerializer.Deserialize<WeatherModel>(json);

                    if (weather != null)
                    {
                        weatherData.Add(weather);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.WriteLine($"Error in FetchWeatherDataAsync for date {date:yyyy-MM-dd}: {ex.Message}");

                    stopwatch.Stop(); // Stop stopwatch after the response is received

                    // Record the response time for each weather request
                    _statisticsService.RecordRequest("Weather", stopwatch.ElapsedMilliseconds);
                }
            }

            return weatherData;
        }
    }
}
