using ApiAggregator.Exceptions;

namespace ApiAggregator.Utilities
{
    public static class InputValidator
    {
        // Validate that startDate is not after endDate
        public static void ValidateDateRange(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("start_date cannot be after end_date.");
            }
        }

        // Validate sortBy parameter (should be one of relevancy, popularity, or publishedAt)
        public static void ValidateSortNewsBy(string sortNewsBy)
        {
            var validSortNewsByOptions = new[] { "relevancy", "popularity", "publishedAt" };
            if (string.IsNullOrWhiteSpace(sortNewsBy) || Array.IndexOf(validSortNewsByOptions, sortNewsBy) == -1)
            {
                throw new ArgumentException("Invalid sortNewsBy value. Valid options are: relevancy, popularity, publishedAt.");
            }
        }


        public static void ValidateSortDateBy(string sortDateBy)
        {
            var validSortDateByOptions = new[] { "asc", "desc" };
            if (string.IsNullOrWhiteSpace(sortDateBy) || Array.IndexOf(validSortDateByOptions, sortDateBy.ToLower()) == -1)
            {
                throw new ArgumentException("Invalid sortDateBy value. Valid options are: asc, desc.");
            }
        }

        // Validate the date format for startDate and endDate (YYYY-MM-DD format)
        public static DateOnly ParseAndValidateDate(string date, string paramName)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                throw new ArgumentException($"The {paramName} cannot be null or empty.");
            }

            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                throw new ArgumentException($"The {paramName} '{date}' is not in the correct format (YYYY-MM-DD).");
            }

            return parsedDate;
        }

        // General validation method to ensure all inputs are valid
        public static void ValidateInputs(string keyword, string sortDateBy, string sortNewsBy)
        {
            // Validate sorting parameters
            ValidateSortDateBy(sortDateBy);
            ValidateSortNewsBy(sortNewsBy);

            // Validate keyword
            if (string.IsNullOrWhiteSpace(keyword))
            {
                throw new ArgumentException("Keyword cannot be null or empty.");
            }
        }
    }
}
