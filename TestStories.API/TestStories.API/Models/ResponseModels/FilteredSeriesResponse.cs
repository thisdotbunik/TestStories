using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class FilteredSeriesResponse
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "type")]
        public string Type { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty(propertyName: "latestPublishedMediaDate")]
        public DateTime? LatestPublishedMediaDate { get; set; }
    }
}
