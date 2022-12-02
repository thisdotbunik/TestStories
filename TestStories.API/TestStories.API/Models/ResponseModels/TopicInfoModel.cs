using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class TopicInfoModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "topicName")]
        public string TopicName { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "topicThumbnail")]
        public string TopicThumbnail { get; set; }

        [JsonProperty(propertyName: "topicThumbnails")]
        public Images TopicThumbnails { get; set; } = new Images();

        [JsonProperty(propertyName: "videoCount")]
        public int VideoCount { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "medias")]
        public List<DataAccess.Entities.Media> Medias { get; set; }
    }
}
