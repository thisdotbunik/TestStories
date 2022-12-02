using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddExperimentModel
    {

        [DefaultValue("")]
        [Required]
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "experimentTypeId")]
        public byte ExperimentTypeId { get; set; }

        [JsonProperty(propertyName: "experimentStatusId")]
        public byte ExperimentStatusId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "startDate")]
        public DateTime? StartDate { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "endDate")]
        public DateTime? EndDate { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "goal")]
        public string Goal { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "videoPlays")]
        public int VideoPlays { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "engagementTypeId")]
        public byte? EngagementTypeId { get; set; }

        [JsonProperty(propertyName: "lstMedia")]
        public ICollection<ExperimentMediaModel> LstMedia { get; set; }
    }
}
