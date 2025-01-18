using ApiAggregator.Interfaces;
using ApiAggregator.Models;
using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using static ApiAggregator.Models.NewsModel;

namespace ApiAggregator.Services
{
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly IStatisticsService _statisticsService;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public NewsService(HttpClient httpClient, IConfiguration configuration, IStatisticsService statisticsService)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ExternalApis:NewsApi:BaseUrl"];
            _apiKey = configuration["ExternalApis:NewsApi:ApiKey"];
            _statisticsService = statisticsService;

            // Initialize Polly retry policy
            _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} for News API: {exception}. Retrying in {timeSpan.TotalSeconds} seconds...");
                });

        }

        public async Task<NewsModel> FetchDataAsync(string keyword, DateOnly startDate, DateOnly endDate, string sortNewsBy)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // Start stopwatch to measure response time

            try
            {
                var url = $"{_baseUrl}?q={keyword}&from={startDate:yyyy-MM-dd}&to={endDate:yyyy-MM-dd}&sortBy={sortNewsBy}";

                _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ApiAggregator/1.0 (+https://github.com/NickSakellariou)");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                // Use Polly retry policy to make the HTTP request
                var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(url));

                stopwatch.Stop(); // Stop stopwatch after the response is received

                // Record the response time
                _statisticsService.RecordRequest("News", stopwatch.ElapsedMilliseconds);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error fetching data: {response.StatusCode}, {errorContent}");

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null; // API returned no data
                    }

                    throw new HttpRequestException($"API call failed: {response.StatusCode}, Details: {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<NewsModel>(json);

                if (data?.Articles != null && data.Articles.Any())
                {
                    // Limit articles to 3 per day
                    data.Articles = data.Articles
                        .GroupBy(article => article.PublishedAt.Date) // Group by date part of PublishedAt
                        .SelectMany(group => group.Take(3)) // Take up to 3 articles per group
                        .ToList();
                }

                return data;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error in FetchDataAsync: {httpEx.Message}");
                stopwatch.Stop(); // Stop stopwatch after the response is received
                _statisticsService.RecordRequest("News", stopwatch.ElapsedMilliseconds);

                // Add error tracking logic (optional, based on your overall architecture)
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in FetchDataAsync: {ex.Message}");
                stopwatch.Stop(); // Stop stopwatch after the response is received
                _statisticsService.RecordRequest("News", stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}
