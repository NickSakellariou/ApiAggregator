using System.Text.Json.Serialization;

namespace ApiAggregator.Models
{
    public class WeatherModel
    {
        [JsonPropertyName("date")]
        public DateOnly Date { get; set; }

        [JsonPropertyName("temperature")]
        public Temperature Temperature { get; set; }
    }

    public class Temperature
    {
        [JsonPropertyName("min")]
        public double Min { get; set; }

        [JsonPropertyName("max")]
        public double Max { get; set; }

        [JsonPropertyName("afternoon")]
        public double Afternoon { get; set; }

        [JsonPropertyName("night")]
        public double Night { get; set; }

        [JsonPropertyName("evening")]
        public double Evening { get; set; }

        [JsonPropertyName("morning")]
        public double Morning { get; set; }
    }

    public class GeoLocation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
