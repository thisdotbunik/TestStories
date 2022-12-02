using System;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class BlogResponse
    {
        [JsonProperty(propertyName:"id")]
        public string Id { get; set; }

        [JsonProperty(propertyName:"title")]
        public string Title { get; set; }

        [JsonProperty(propertyName:"description")]
        public string Description { get; set; }

        [JsonProperty(propertyName:"featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty(propertyName:"url")]
        public string Url { get; set; }

        [JsonProperty(propertyName:"fetchedAt")]
        public DateTime FetchedAt { get; set; }

        [JsonProperty(propertyName: "publishedDate")]
        public DateTime PublishedDate { get; set; }
    }
}
