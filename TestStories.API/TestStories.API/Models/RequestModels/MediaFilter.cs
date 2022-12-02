using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class MediaFilter
    {
        [DefaultValue("")]
        [JsonProperty(propertyName: "mediaStatus")]
        public string MediaStatus { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "mediaType")]
        public string MediaType { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "topicName")]
        public string TopicName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "seriesName")]
        public string SeriesName { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "source")]
        public string Source { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "publishedBy")]
        public string PublishedBy { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "uploadedBy")]
        public string UploadedBy { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "publishFromDate")]
        public string PublishFromDate { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "publishToDate")]
        public string PublishToDate { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "sortedProperty")]
        public string SortedProperty { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "sortOrder")]
        public string SortOrder { get; set; }
    }
}
