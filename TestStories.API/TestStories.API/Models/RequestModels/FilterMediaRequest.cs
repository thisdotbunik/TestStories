using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class FilterMediaRequest
	{
        [DefaultValue("")]
        [JsonProperty(propertyName: "filterstring")]
        public string Filterstring { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "status")]
        public string Status { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "mediaType")]
        public string MediaType { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "topictitle")]
        public string TopicName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "seriestitle")]
        public string SeriesName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "source")]
        public string Source { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "publishUser")]
        public string PublishUser { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "uploadUser")]
        public string UploadUser { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "publishfromdate")]
        public DateTime? PublishFromDate { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "publishtodate")]
        public DateTime? PublishToDate { get; set; }

        [DefaultValue(1)]
        [JsonProperty(propertyName: "page")]
        public int Page { get; set; }

        [DefaultValue(10)]
        [JsonProperty(propertyName: "pageSize")]
        public int PageSize { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "sortedProperty")]
        public string SortedProperty { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "sortOrder")]
        public string SortOrder { get; set; }
    }
}
