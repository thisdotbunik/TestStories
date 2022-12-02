using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class UpdateExperimentModel
    {
        [JsonProperty(propertyName: "experimentId")]
        [Required]
        public int ExperimentId { get; set; }

        [JsonProperty(propertyName: "statusId")]
        [Required]
        public byte StatusId { get; set; }
    }
}
