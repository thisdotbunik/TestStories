using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class SubscribeTopicModel
    {
        [JsonProperty(propertyName: "subscriptionId")]
        public int SubscriptionId { get; set; }

        [JsonProperty(propertyName: "topicId")]
        public int TopicId { get; set; }

        [JsonProperty(propertyName: "userId")]
        public int UserId { get; set; }
    }
}
