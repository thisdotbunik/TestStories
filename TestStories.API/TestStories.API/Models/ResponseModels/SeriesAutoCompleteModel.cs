using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class SeriesAutoCompleteModel
    {
        [JsonProperty(propertyName: "seriesName")]
        public string SeriesName { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }
    }
}
