using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class PartnerViewModel
    {
        public  List<MediaPartnerViewModel> Items { get; set; }

        [JsonProperty(propertyName: "partnerName")]
        public string PartnerName { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }

        [JsonProperty(propertyName: "count")]
        public int Count { get; set; }
    }

    public class MediaPartnerViewModel
    {
        [JsonProperty(propertyName: "id")]
        public long Id { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "status")]
        public string Status { get; set; }

        [JsonProperty(propertyName: "mediatype")]
        public string MediaType { get; set; }

        [JsonProperty(propertyName: "publishdate")]
        public DateTime PublishDate { get; set; }

        [JsonProperty(propertyName: "source")]
        public string Source { get; set; }

        [JsonProperty(propertyName: "editor")]
        public string Editor { get; set; }

        [JsonProperty(propertyName: "publishby")]
        public string PublishBy { get; set; }

        [JsonProperty(propertyName: "isVisibleOnGoogle")]
        public bool IsVisibleOnGoogle { get; set; }
    }
}
