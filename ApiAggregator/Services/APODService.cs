using ApiAggregator.Interfaces;
using ApiAggregator.Models;
using ApiAggregator.Utilities;
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
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var url = $"{_baseUrl}?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}";
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                // Use the Polly retry policy
                var response = await PollyPolicies.GetRetryPolicy().ExecuteAsync(() => _httpClient.GetAsync(url));

                stopwatch.Stop();
                _statisticsService.RecordRequest("APOD", stopwatch.ElapsedMilliseconds);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error fetching data: {response.StatusCode}, {errorContent}");
                    return new List<APODModel>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<APODModel>>(json) ?? new List<APODModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                stopwatch.Stop();
                _statisticsService.RecordRequest("APOD", stopwatch.ElapsedMilliseconds);
                return new List<APODModel>();
            }
        }

    }
}
