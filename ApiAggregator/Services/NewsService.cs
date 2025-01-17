using ApiAggregator.Interfaces;
using ApiAggregator.Models;
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

        public NewsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ExternalApis:NewsApi:BaseUrl"];
            _apiKey = configuration["ExternalApis:NewsApi:ApiKey"];
        }

        public async Task<NewsModel> FetchDataAsync(string keyword, DateOnly startDate, DateOnly endDate, string sortNewsBy)
        {
            try
            {
                var url = $"{_baseUrl}?q={keyword}&from={startDate:yyyy-MM-dd}&to={endDate:yyyy-MM-dd}&sortBy={sortNewsBy}";

                _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ApiAggregator/1.0 (+https://github.com/NickSakellariou)");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                // Make the HTTP request
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
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in FetchDataAsync: {ex.Message}");
                throw;
            }
        }
    }
}
