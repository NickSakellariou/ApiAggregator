using ApiAggregator.Interfaces;
using ApiAggregator.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiAggregator.Services
{
    public class APODService : IAPODService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly IStatisticsService _statisticsService;

        public APODService(HttpClient httpClient, IConfiguration configuration, IStatisticsService statisticsService)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ExternalApis:APODApi:BaseUrl"];
            _apiKey = configuration["ExternalApis:APODApi:ApiKey"];
            _statisticsService = statisticsService;
        }

        public async Task<List<APODModel>> FetchDataAsync(DateOnly startDate, DateOnly endDate)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start stopwatch to measure response time

            try
            {
                var url = $"{_baseUrl}?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}";

                _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                var response = await _httpClient.GetAsync(url);

                stopwatch.Stop(); // Stop stopwatch after the response is received

                // Record the response time
                _statisticsService.RecordRequest("APOD", stopwatch.ElapsedMilliseconds);

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
                var dataList = JsonSerializer.Deserialize<List<APODModel>>(json);

                if (dataList == null || dataList.Count == 0)
                {
                    throw new InvalidOperationException("Failed to deserialize APOD API response or response is empty.");
                }

                foreach (var item in dataList)
                {
                    Console.WriteLine($"Title: {item.Title}, Date: {item.Date}");
                }

                return dataList;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error in FetchDataAsync: {httpEx.Message}");
                stopwatch.Stop();
                _statisticsService.RecordRequest("APOD", stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in FetchDataAsync: {ex.Message}");
                stopwatch.Stop();
                _statisticsService.RecordRequest("APOD", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
