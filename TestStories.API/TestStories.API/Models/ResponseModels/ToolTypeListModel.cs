using System;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ToolTypeListModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDateUtc { get; set; }
    }
}
