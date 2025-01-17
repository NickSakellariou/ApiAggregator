using System.Text.Json.Serialization;

namespace ApiAggregator.Models
{
    public class NewsModel
    {
        [JsonPropertyName("articles")]
        public List<Article> Articles { get; set; }

        public class Article
        {
            [JsonPropertyName("author")]
            public string Author { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("publishedAt")]
            public DateTime PublishedAt { get; set; }
        }
    }
}
