using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class CollectionModel<T>
    {
        [JsonProperty(propertyName: "items")]
        public IEnumerable<T> Items { get; set; }

        [JsonProperty(propertyName: "totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty(propertyName:"pageNumber")]
        public int PageNumber { get; set; }

        [JsonProperty(propertyName:"pageSize")]
        public int PageSize { get; set; }
    }
}
