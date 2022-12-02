using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class MediaListResponse
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "topic")]
        public List<string> Topic { get; set; }

        [JsonProperty(propertyName: "mediaType")]
        public string MediaType { get; set; }

        [JsonProperty(propertyName: "mediaStatus")]
        public string MediaStatus { get; set; }

        [JsonProperty(propertyName: "publishedBy")]
        public string PublishedBy { get; set; }

        [JsonProperty(propertyName: "series")]
        public string Series { get; set; }

        [JsonProperty(propertyName: "source")]
        public string Source { get; set; }

        [JsonProperty(propertyName: "mediaTools")]
        public List<string> MediaTools { get; set; }

        [JsonProperty(propertyName: "publishDate")]
        public DateTime? PublishDate { get; set; }
        public int? UploadedById { get; set; }

        [JsonProperty(propertyName: "uploadedByUser")]
        public string UploadedByUser { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "isVisibleOnGoogle")]
        public bool IsVisibleOnGoogle { get; set; }
    }
}
