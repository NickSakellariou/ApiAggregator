using ApiAggregator.Models;

namespace ApiAggregator.Interfaces
{
    public interface INewsService
    {
        Task<NewsModel> FetchDataAsync(string keyword, DateOnly startDate, DateOnly endDate, string sortNewsBy);
    }
}
