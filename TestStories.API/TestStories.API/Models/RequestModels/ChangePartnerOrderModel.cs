using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class ChangePartnerOrderModel
    {
        [JsonProperty(propertyName: "partnerId")]
        [Required]
        public int PartnerId { get; set; }

        [JsonProperty(propertyName: "orderNumber")]
        [Required]
        public int OrderNumber { get; set; }
    }
}
