using System;
using System.Collections.Generic;
using TestStories.API.Models.RequestModels;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ExperimentListModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "experimentType")]
        public string ExperimentType { get; set; }

        [JsonProperty(propertyName: "experimentStatus")]
        public string ExperimentStatus { get; set; }

        [JsonProperty(propertyName: "startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty(propertyName: "endDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty(propertyName:"createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(propertyName: "goal")]
        public string Goal { get; set; }

        [JsonProperty(propertyName: "hypothesisMediaId")]
        public long? HypothesisMediaId { get; set; }

        [JsonProperty(propertyName: "videoPlays")]
        public int? VideoPlays { get; set; }

        [JsonProperty(propertyName: "engagementTypeId")]
        public byte? EngagementTypeId { get; set; }

        [JsonProperty(propertyName: "medias")]
        public ICollection<ExperimentMediaModel> Medias { get; set; }
    }
}
