using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class TopicViewModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "topicName")]
        public string TopicName { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "parentTopic")]
        public string ParentTopic { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "parentId")]
        public int ParentId { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "featuredImage")]
        public string FeaturedImage { get; set; }
    }
}
