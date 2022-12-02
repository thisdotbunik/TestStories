using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class OptimisedBlogResponse: AllFeaturedResponse<string>
    {
        [JsonProperty(propertyName: "publishedDate")]
        public DateTime PublishedDate { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }
    }
}
