using ApiAggregator.Interfaces;
using ApiAggregator.Middleware;
using ApiAggregator.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(); // Loads user secrets
}

// Add services to the container.

builder.Services.AddControllers();

// Dependency Injection
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddHttpClient<NewsService>();
builder.Services.AddHttpClient<APODService>();

builder.Services.AddScoped<IAPODService, APODService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IAggregationService, AggregationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Aggregator",
        Version = "v1",
        Description = "An API that aggregates data from multiple sources."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
