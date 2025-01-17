using ApiAggregator.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiAggregator.Interfaces
{
    public interface IAPODService
    {
        Task<List<APODModel>> FetchDataAsync(DateOnly startDate, DateOnly endDate);
    }
}
