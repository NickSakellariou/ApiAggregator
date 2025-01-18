using Polly.Retry;
using Polly;

namespace ApiAggregator.Utilities
{
    public static class PollyPolicies
    {
        public static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy<HttpResponseMessage> // Specify the type here explicitly
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount}: {exception}. Retrying in {timeSpan.TotalSeconds}s...");
                    });
        }
    }
}
