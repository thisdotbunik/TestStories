using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddSendToPartnerModel
    {
        [JsonProperty(propertyName: "partnerId")]
        [Required]
        public int PartnerId { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        [Required]
        public long MediaId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "email")]
        [Required]
        public string Email { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "startDate")]
        [Required]
        public string StartDate { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "endDate")]
        [Required]
        public string EndDate { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "message")]
        public string Message { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "encMediaId")]
        public string EncMediaId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "encPartnerId")]
        public string EncPartnerId { get; set; }
    }
}
