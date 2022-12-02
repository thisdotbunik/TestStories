using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class FilterClientToolViewRequest
    {
        [DefaultValue(1)]
        [JsonProperty(propertyName: "page")]
        public int Page { get; set; }

        [DefaultValue(10)]
        [JsonProperty(propertyName: "pageSize")]
        public int PageSize { get; set; }
    }
}
