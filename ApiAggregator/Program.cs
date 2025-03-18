using ApiAggregator.Decorators;
using ApiAggregator.Interfaces;
using ApiAggregator.Middleware;
using ApiAggregator.Services;
using Microsoft.Extensions.Caching.Distributed;

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

// Register the concrete AggregationService.
builder.Services.AddTransient<AggregationService>();

// Register the caching decorator as the IAggregationService.
builder.Services.AddTransient<IAggregationService>(sp =>
    new AggregationServiceCachingDecorator(
        sp.GetRequiredService<AggregationService>(),
        sp.GetRequiredService<IDistributedCache>()
    )
);

builder.Services.AddSingleton<IStatisticsService, StatisticsService>();

// Register the distributed cache implementation.
builder.Services.AddDistributedMemoryCache();

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

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
