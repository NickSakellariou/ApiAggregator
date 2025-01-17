using ApiAggregator.Models;

namespace ApiAggregator.Interfaces
{
    public interface IWeatherService
    {
        Task<List<WeatherModel>> FetchDataAsync(string city, DateOnly startDate, DateOnly endDate);
        Task<GeoLocation> GetLocationAsync(string city);
        Task<List<WeatherModel>> FetchWeatherDataAsync(double lat, double lon, DateOnly startDate, DateOnly endDate);
    }
}
