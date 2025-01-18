# API Aggregation

## Overview
The **API Aggregation Service** is a .NET-based application that consolidates data from multiple external APIs into a unified JSON response. It demonstrates effective API integration, asynchronous programming, error handling, and best practices for scalability and performance.

---

## External APIs Used

1. **OpenWeatherMap API**
   - Provides weather data for various locations worldwide.
   - [Documentation](https://openweathermap.org/api)

2. **News API**
   - Offers access to headlines and articles from global news sources.
   - [Documentation](https://newsapi.org/)

3. **Astronomy Picture of the Day (APOD) API**
   - Supplies daily images and metadata from NASA’s APOD service.
   - [Documentation](https://github.com/nasa/apod-api)

---

## Input Parameters

The API accepts the following query parameters:

| Parameter    | Type     | Description                                                | Example Value      |
|--------------|----------|------------------------------------------------------------|--------------------|
| `startDate`  | DateOnly | The start date for data aggregation.                        | `2025-01-10`       |
| `endDate`    | DateOnly | The end date for data aggregation.                          | `2025-01-11`       |
| `keyword`    | string   | A keyword to filter news articles.                         | `Athens`           |
| `sortDateBy` | string   | Specifies how to sort the results by date (`asc` or `desc`).| `asc`              |
| `sortNewsBy` | string   | Specifies how to sort news articles (`relevance`, `popularity`).| `popularity`     |

### Notes:
- Dates must be provided in the `YYYY-MM-DD` format.
- Ensure all input parameters are valid; otherwise, the API may return an error.

---

## Expected Output Format

The API returns a JSON array where each object represents a single date and its aggregated data:

```json
[
  {
    "Date": "2025-01-10",
    "Weather": {
      "min": 12.12,
      "max": 18.18,
      "afternoon": 17.32,
      "night": 12.88,
      "evening": 15.52,
      "morning": 12.39
    },
    "AstronomyPictureOfTheDay": {
      "copyright": "Long Xin",
      "date": "2025-01-10",
      "explanation": "An unassuming region in the constellation Taurus holds these dark and dusty nebulae...",
      "media_type": "image",
      "title": "Young Stars, Dark Nebulae",
      "url": "https://apod.nasa.gov/apod/image/2501/B209V773Tau_1024.png"
    },
    "News": {
      "articles": [
        {
          "author": "Joe Hotchkiss, Augusta Chronicle",
          "title": "Snow, ice shuts down Richmond, Columbia counties. Here's what's closed",
          "description": "Thinking of driving on icy, slushy roads? Think again.",
          "url": "https://www.augustachronicle.com/story/weather/2025/01/10/winter-storm...",
          "publishedAt": "2025-01-10T16:59:03Z"
        },
        ...
      ]
    }
  },
  ...
]
```

## Web Caching

To optimize performance and reduce redundant API calls, the project uses in-memory caching. The caching mechanism works as follows:

1. **Cache Key Generation**:
   - A unique cache key is generated based on the query parameters (e.g., `startDate`, `endDate`, `keyword`, `sortDateBy`, `sortNewsBy`).

2. **Cache Lookup**:
   - Before making an API call, the application checks if the response for the given query already exists in the cache.

3. **Cache Expiry**:
   - Cached responses are stored for **5 minutes** (absolute expiration).
   - If no activity occurs during a **2-minute sliding window**, the cache entry is removed.

4. **Thread Safety**:
   - The caching logic is implemented alongside a **semaphore** to limit concurrent access to the same resource.

## Parallelism

To enhance performance and decrease response times, the project utilizes parallelism when fetching data from external APIs. The parallel processing mechanism works as follows:

1. **Concurrent API Calls**:
   - Requests to the external APIs (OpenWeatherMap, News API, and APOD API) are executed concurrently using asynchronous programming patterns.
   - This minimizes the total response time by avoiding sequential API calls.

2. **Efficient Task Management**:
   - The project leverages `Task.WhenAll` to aggregate the results of all API calls, ensuring that each API is queried independently but simultaneously.

3. **Scalability**:
   - The use of parallelism allows the application to scale efficiently, handling multiple concurrent requests without blocking resources.

4. **Error Handling**:
   - Each API call is wrapped with proper error handling to ensure that failures in one API do not affect the responses from others.

By combining parallel API calls with efficient caching, the application achieves a significant reduction in response times, making it both fast and robust.


## Unit Tests

This project includes a comprehensive suite of unit tests to ensure functionality and reliability:

- **Null Responses Handling**: Verifies the system can handle null responses gracefully without breaking.
- **Data Integration Tests**: Ensures data from all three APIs is aggregated correctly.
- **Sorting and Filtering**: Tests the API's ability to sort and filter data as specified by the query parameters.
- **Error Handling**: Simulates API failures to verify fallback mechanisms and error responses.

---

## Running the Project

### Clone the Repository

To start working with the project, first clone the repository to your local machine:

```bash
git clone https://github.com/NickSakellariou/ApiAggregator.git
cd ApiAggregator
```

## Set Up API Keys

Before running the project, set up the necessary API keys in the `secrets.json` configuration file. This file contains the details for the external APIs:

```json
{
  "ExternalApis": {
    "OpenWeatherMap": {
      "GeoUrl": "http://api.openweathermap.org/geo/1.0/direct",
      "BaseUrl": "https://api.openweathermap.org/data/3.0/onecall/day_summary",
      "ApiKey": "yourApiKey"
    },
    "NewsApi": {
      "BaseUrl": "https://newsapi.org/v2/everything",
      "ApiKey": "yourApiKey"
    },
    "APODApi": {
      "BaseUrl": "https://api.nasa.gov/planetary/apod",
      "ApiKey": "yourApiKey"
    }
  }
}
```
Make sure to replace the API keys in the `secrets.json` file with your actual keys if necessary.

---

## Build and Run the Project

Now, build and run the project using the following commands:

```bash
dotnet build
dotnet run
```

## Access the API

Once the application is running, you can access the API at the following endpoint:

```http
GET https://localhost:7054/api/apiaggregation/aggregate?startDate=2025-01-10&endDate=2025-01-11&keyword=Athens&sortDateBy=asc&sortNewsBy=popularity
```

You can modify the query parameters (startDate, endDate, keyword, etc.) based on your needs.
