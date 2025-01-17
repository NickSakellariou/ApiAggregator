using ApiAggregator.Models;

namespace ApiAggregator.Interfaces
{
    public interface IAggregationService
    {
        Task<string> GetAggregatedResults(DateOnly startDate, DateOnly endDate, string keyword, string sortDateBy, string sortNewsBy);

    }
}
