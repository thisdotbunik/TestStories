using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class OptimisedSeriesResponse: AllFeaturedResponse<int>
    {
        [JsonProperty(propertyName: "seriesType")]
        public string SeriesType { get; set; }

        [JsonProperty(propertyName: "mediaTypeId")]
        public int? MediaTypeId { get; set; }

        [JsonProperty(propertyName: "medias")]
        public ICollection<DataAccess.Entities.Media> Medias { get; set; }

    }
}
