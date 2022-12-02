using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class FilteredResourceModel
    {
        [JsonProperty(propertyName:"type")]
        public string Type { get; set; }

        [JsonProperty(propertyName:"data")]
        public List<EntityModel> Data { get; set; }
    }
}
