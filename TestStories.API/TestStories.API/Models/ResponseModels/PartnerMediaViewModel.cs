using System;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class PartnerMediaViewModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "mediaStatus")]
        public string MediaStatus { get; set; }

        [JsonProperty(propertyName: "createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty(propertyName: "publishDate")]
        public DateTime? PublishDate { get; set; }

        [JsonProperty(propertyName: "uploadedByUser")]
        public string UploadedByUser { get; set; }

        [JsonProperty(propertyName: "isVisibleOnGoogle")]
        public bool IsVisibleOnGoogle { get; set; }
    }
}
