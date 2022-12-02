using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class ExportResourceFilter
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
    }
}
