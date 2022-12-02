using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class SubscribeSeriesModel
    {
        [JsonProperty(propertyName: "subscriptionId")]
        public int SubscriptionId { get; set; }

        [JsonProperty(propertyName: "seriesId")]
        public int SeriesId { get; set; }

        [JsonProperty(propertyName: "userId")]
        public int UserId { get; set; }
    }
}
