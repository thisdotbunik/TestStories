using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ShortSeriesModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "seriesTitle")]
        public string SeriesTitle { get; set; }

        [JsonProperty(propertyName: "showOnMenu")]
        public bool ShowOnMenu { get; set; }
    }
}
