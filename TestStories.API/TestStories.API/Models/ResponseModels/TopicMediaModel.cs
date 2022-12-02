using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class TopicMediaModel
    {
        [JsonProperty(propertyName: "topicId")]
        public int TopicId { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }

        [JsonProperty(propertyName: "logos")]
        public Images Logos { get; set; } = new Images();

        [JsonProperty(propertyName: "videoCount")]
        public int VideoCount { get; set; }

        [JsonProperty(propertyName: "videos")]
        public List<MediaInfoModel> Videos { get; set; }

        [JsonProperty(propertyName: "seUrl")]
        public string SeoUrl { get; set; }
    }
}
