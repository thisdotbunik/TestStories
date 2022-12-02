using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class PartnerOrderModel
    {
        [JsonProperty(propertyName: "partnersOrdering")]
        public ICollection<ChangePartnerOrderModel> PartnersOrdering { get; set; } = new List<ChangePartnerOrderModel>();
    }
}
