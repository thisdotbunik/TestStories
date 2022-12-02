using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ShortToolModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string ToolName { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "type")]
        public string Type { get; set; }

        [JsonProperty(propertyName: "featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty(propertyName: "featuredImages")]
        public Images FeaturedImages { get; set; } = new Images();

        [JsonProperty(propertyName: "showOnMenu")]
        public bool? ShowOnMenu { get; set; }

        [JsonProperty(propertyName: "showOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [JsonProperty(propertyName: "assignto")]
        public List<string> AssignmentForCloud { get; set; }

        [JsonProperty(propertyName: "topicNames")]
        public List<string> TopicNames { get; set; }
    }

    public class AssignToItem
    {
        [JsonProperty(propertyName: "media")]
        public List<string> Media { get; set; }

        [JsonProperty(propertyName: "series")]
        public List<string> Series { get; set; }
    }
}

