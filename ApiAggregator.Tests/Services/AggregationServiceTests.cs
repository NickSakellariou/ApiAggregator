﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ApiAggregator.Interfaces;
using ApiAggregator.Models;
using ApiAggregator.Services;
using ApiAggregator.Tests.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace ApiAggregator.Tests.Services
{
    public class AggregationServiceTests
    {
        private readonly Mock<IAPODService> _mockApodService;
        private readonly Mock<INewsService> _mockNewsService;
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly AggregationService _aggregationService;

        public AggregationServiceTests()
        {
            _mockApodService = new Mock<IAPODService>();
            _mockNewsService = new Mock<INewsService>();
            _mockWeatherService = new Mock<IWeatherService>();

            _aggregationService = new AggregationService(
                _mockApodService.Object,
                _mockNewsService.Object,
                _mockWeatherService.Object
            );
        }

        [Fact]
        public async Task GetAggregatedResults_ReturnsAggregatedData()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)).ToString("yyyy-MM-dd");
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).ToString("yyyy-MM-dd");
            var keyword = "test";
            var sortDateBy = "asc";
            var sortNewsBy = "popularity";

            var parsedStartDate = DateOnly.ParseExact(startDate, "yyyy-MM-dd");
            var parsedEndDate = DateOnly.ParseExact(endDate, "yyyy-MM-dd");

            // Mock APODService
            _mockApodService
                .Setup(s => s.FetchDataAsync(parsedStartDate, parsedEndDate))
                .ReturnsAsync(new List<APODModel>
                {
                    new APODModel
                    {
                        Date = parsedStartDate.AddDays(1).ToString("yyyy-MM-dd"),
                        Title = "Test Title",
                        Explanation = "Test Explanation",
                        MediaType = "image",
                        Url = "http://example.com/test.jpg"
                    }
                });

            // Mock WeatherService
            _mockWeatherService
                .Setup(s => s.FetchDataAsync(keyword, parsedStartDate, parsedEndDate))
                .ReturnsAsync(new List<WeatherModel>
                {
                    new WeatherModel
                    {
                        Date = parsedStartDate.AddDays(1),
                        Temperature = new Temperature
                        {
                            Morning = 10,
                            Afternoon = 25,
                            Evening = 20,
                            Night = 15,
                            Min = 10,
                            Max = 25
                        }
                    }
                });

            // Mock NewsService
            _mockNewsService
                .Setup(s => s.FetchDataAsync(keyword, parsedStartDate, parsedEndDate, sortNewsBy))
                .ReturnsAsync(new NewsModel
                {
                    Articles = new List<NewsModel.Article>
                    {
                        new NewsModel.Article
                        {
                            Title = "Test Article",
                            PublishedAt = DateTime.UtcNow.AddDays(-2),
                            Author = "Test Author",
                            Description = "Test Description",
                            Url = "http://example.com/test-article"
                        }
                    }
                });

            // Act
            var resultJson = await _aggregationService.GetAggregatedResults(startDate, endDate, keyword, sortDateBy, sortNewsBy);

            // Deserialize JSON result into AggregatedResponseWrapper
            var resultWrapper = JsonSerializer.Deserialize<AggregatedResponseWrapper>(resultJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            resultWrapper.Should().NotBeNull();
            resultWrapper.Data.Should().NotBeNull();
            resultWrapper.Data.Count.Should().Be(3); // 3 days in the range

            var day2 = resultWrapper.Data.FirstOrDefault(r => r.Date == parsedStartDate.AddDays(1));
            day2.Should().NotBeNull();
            day2.Weather.Should().NotBeNull();
            day2.Weather.Max.Should().Be(25);
            day2.Weather.Min.Should().Be(10);
            day2.AstronomyPictureOfTheDay.Title.Should().Be("Test Title");
            day2.News.Articles.Should().HaveCount(1);
            day2.News.Articles.First().Title.Should().Be("Test Article");
        }

        [Fact]
        public async Task GetAggregatedResults_HandlesNullResponses()
        {
            // Arrange
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)).ToString("yyyy-MM-dd");
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).ToString("yyyy-MM-dd");
            var keyword = "test";
            var sortDateBy = "asc";
            var sortNewsBy = "popularity";

            var parsedStartDate = DateOnly.ParseExact(startDate, "yyyy-MM-dd");
            var parsedEndDate = DateOnly.ParseExact(endDate, "yyyy-MM-dd");

            _mockApodService.Setup(s => s.FetchDataAsync(parsedStartDate, parsedEndDate)).ReturnsAsync((List<APODModel>)null);
            _mockWeatherService.Setup(s => s.FetchDataAsync(keyword, parsedStartDate, parsedEndDate)).ReturnsAsync((List<WeatherModel>)null);
            _mockNewsService.Setup(s => s.FetchDataAsync(keyword, parsedStartDate, parsedEndDate, sortNewsBy)).ReturnsAsync((NewsModel)null);

            // Act
            var resultJson = await _aggregationService.GetAggregatedResults(startDate, endDate, keyword, sortDateBy, sortNewsBy);

            // Deserialize the result JSON
            var resultWrapper = JsonSerializer.Deserialize<UnifiedResponse<List<AggregatedResponse>>>(resultJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            resultWrapper.Should().NotBeNull();
            resultWrapper.Data.Should().NotBeNull();
            resultWrapper.Data.Count.Should().Be(3); // Three days in the range, even if all API responses are null

            foreach (var response in resultWrapper.Data)
            {
                response.Weather.Should().BeNull(); // No weather data
                response.AstronomyPictureOfTheDay.Should().BeNull(); // No APOD data
                response.News.Should().NotBeNull(); // NewsModel is initialized
                response.News.Articles.Should().BeEmpty(); // Articles are empty
            }
        }
    }
}
