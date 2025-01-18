using ApiAggregator.Models;

namespace ApiAggregator.Interfaces
{
    public interface IAggregationService
    {
        Task<string> GetAggregatedResults(string startDate, string endDate, string keyword, string sortDateBy, string sortNewsBy);

    }
}
