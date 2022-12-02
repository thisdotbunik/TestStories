using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class FilterRequestModel
    {
        [JsonProperty(propertyName: "types")]
        public string[] Types { get; set; }

        [JsonProperty(propertyName: "partners")]
        public string[] Partners { get; set; }

        [JsonProperty(propertyName: "topics")]
        public string[] Topics { get; set; }

        [JsonProperty(propertyName: "pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonProperty(propertyName: "pageNumber")]
        public int PageNumber { get; set; } = 1;

        [JsonProperty(propertyName: "sort")]
        public string Sort { get; set; } = "datecreated";

        [JsonProperty(propertyName: "order")]
        public string Order { get; set; } = "desc";
    }
}
