using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class FilterTopicViewRequest 
    {
        [DefaultValue("")]
        [JsonProperty(propertyName: "filterString")]
        public string FilterString { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "sortedProperty")]
        public string SortedProperty { get; set; }

        [DefaultValue("ascending")]
        [JsonProperty(propertyName: "sortOrder")]
        public string SortOrder { get; set; }

        [DefaultValue(1)]
        [JsonProperty(propertyName: "page")]
        public int Page { get; set; }

        [DefaultValue(10)]
        [JsonProperty(propertyName: "pageSize")]
        public int PageSize { get; set; }
    }
}
