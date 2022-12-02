using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SeriesFilterRequest
    {
        [JsonProperty(propertyName: "pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonProperty(propertyName: "pageNumber")]
        public int PageNumber { get; set; } = 1;

        [JsonProperty(propertyName: "order")]
        public string Order { get; set; } = "asc";
    }
}
