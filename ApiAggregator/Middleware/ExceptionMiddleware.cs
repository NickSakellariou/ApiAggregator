using ApiAggregator.Exceptions;

namespace ApiAggregator.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, ex, 400);
            }
            catch (ValidationException ex)
            {
                await HandleExceptionAsync(context, ex, 400);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, 500);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                code = statusCode,
                message = exception.Message
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }

}
