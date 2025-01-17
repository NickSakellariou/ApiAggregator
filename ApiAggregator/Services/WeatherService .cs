using ApiAggregator.Models;
using ApiAggregator.Interfaces;
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

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _geoUrl = configuration["ExternalApis:OpenWeatherMap:GeoUrl"];
            _baseUrl = configuration["ExternalApis:OpenWeatherMap:BaseUrl"];
            _apiKey = configuration["ExternalApis:OpenWeatherMap:ApiKey"];
        }

        public async Task<List<WeatherModel>> FetchDataAsync(string city, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                // Step 1: Get Latitude and Longitude for the city
                var geoLocation = await GetLocationAsync(city);
                if (geoLocation == null)
                {
                    return null;
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
            try
            {
                var url = $"{_geoUrl}?q={city}";

                _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                var response = await _httpClient.GetAsync(url);

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
                return null;
            }
        }

        public async Task<List<WeatherModel>> FetchWeatherDataAsync(double lat, double lon, DateOnly startDate, DateOnly endDate)
        {
            var weatherData = new List<WeatherModel>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                try
                {
                    var weatherUrl = $"{_baseUrl}?lat={lat}&lon={lon}&date={date:yyyy-MM-dd}&units=metric";

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                    var response = await _httpClient.GetAsync(weatherUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Weather API call failed for {date:yyyy-MM-dd} with status code: {response.StatusCode}");
                        continue; // Skip to the next date
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        // Log the error for monitoring
                        Console.WriteLine($"Weather API call failed for {date:yyyy-MM-dd} with status code: {response.StatusCode}, {errorContent}");

                        // Decide based on status code
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            continue; // Skip to the next date, API returned no data, but it's not critical
                        }

                        throw new HttpRequestException($"API call failed: {response.StatusCode}, Details: {errorContent}");
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
                }
            }

            return weatherData;
        }
    }
}
