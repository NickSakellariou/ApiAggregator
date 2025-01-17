using ApiAggregator.Exceptions;

namespace ApiAggregator.Utilities
{
    public static class InputValidator
    {
        public static void ValidateDateRange(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("start_date cannot be after end_date.");
            }
        }
    }
}
