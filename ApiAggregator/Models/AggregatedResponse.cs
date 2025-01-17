using static ApiAggregator.Models.NewsModel;

namespace ApiAggregator.Models
{
    public class AggregatedResponse
    {
        public DateOnly Date { get; set; }
        public Temperature Weather { get; set; }
        public APODModel AstronomyPictureOfTheDay { get; set; }
        public NewsModel News { get; set; }
    }
}
