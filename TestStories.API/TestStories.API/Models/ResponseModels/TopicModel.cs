using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class TopicModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "topicName")]
        public string TopicName { get; set; }

        [JsonProperty(propertyName: "parentId")]
        public int? ParentId { get; set; }

        [JsonProperty(propertyName: "parentTopic")]
        public string ParentTopic { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }

        [JsonProperty(propertyName: "logos")]
        public Images Logos { get; set; } = new Images();

        [JsonProperty(propertyName: "logoFileName")]
        public string LogoFileName { get; set; }

        [JsonProperty(propertyName: "statusAddedOnCloud")]
        public string StatusAddedOnCloud { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }
    }
}
