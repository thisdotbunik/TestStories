using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class OptimisedVideosResponse: AllFeaturedResponse<long>
    {
        [JsonProperty(propertyName: "seriesType")]
        public string SeriesType { get; set; }

        [JsonProperty(propertyName: "series")]
        public string Series { get; set; }

        [JsonProperty(propertyName: "topic")]
        public string Topic { get; set; }

        [JsonProperty(propertyName: "mediaTypeId")]
        public int MediaTypeId { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty("mediaMetadata")]
        public string MediaMetadata { get; set; }

        [JsonProperty("mediaDuration")]
        public string MediaDuration { get; set; }

        [JsonProperty("resourceTitle")]
        public string ResourceTitle { get; set; }

        [JsonProperty("resourceUrl")]
        public string ResourceUrl { get; set; }
    }
}
