using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestStories.API.Models.ResponseModels;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class OptimisedSeriesRequest: AllFeaturedResponse<int>
    {
        [JsonProperty(propertyName: "seriesType")]
        public string SeriesType { get; set; }

        [JsonProperty(propertyName: "mediaTypeId")]
        public int? MediaTypeId { get; set; }
    }
}
