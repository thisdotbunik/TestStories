using System;
using System.Collections.Generic;
using TestStories.API.Models.RequestModels;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class ExperimentViewModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "experimentTypeId")]
        public int ExperimentTypeId { get; set; }

        [JsonProperty(propertyName: "experimentStatusId")]
        public int ExperimentStatusId { get; set; }

        [JsonProperty(propertyName: "createdUserId")]
        public int CreatedUserId { get; set; }

        [JsonProperty(propertyName: "startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty(propertyName: "endDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty(propertyName: "goal")]
        public string Goal { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        public long? MediaId { get; set; }

        [JsonProperty(propertyName: "videoPlays")]
        public int? VideoPlays { get; set; }

        [JsonProperty(propertyName: "engagementTypeId")]
        public byte? EngagementTypeId { get; set; }

        [JsonProperty(propertyName: "lstMedia")]
        public ICollection<ExperimentMediaModel> LstMedia { get; set; }
    }
}
