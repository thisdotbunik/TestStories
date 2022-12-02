using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SearchRequest
    {
        [JsonProperty(propertyName: "searchString")]
        [DefaultValue("")]
        public string SearchString { get; set; } = "";
    }
}
