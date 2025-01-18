namespace ApiAggregator.Interfaces
{
    public interface IStatisticsService
    {
        void RecordRequest(string apiName, long responseTime);
        List<object> GetStatistics();
    }
}
