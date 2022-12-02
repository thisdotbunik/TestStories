using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class PartnerMediaFilterRequest
    {
        [JsonProperty(propertyName:"partnerId")]
        [Required]
        public int PartnerId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "filterString")]
        public string FilterString { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "sortedProperty")]
        public string SortedProperty { get; set; }

        [DefaultValue("ascending")]
        [JsonProperty(propertyName: "sortOrder")]
        public string SortOrder { get; set; }

        [DefaultValue(1)]
        [JsonProperty(propertyName: "page")]
        public int Page { get; set; }

        [DefaultValue(10)]
        [JsonProperty(propertyName: "pageSize")]
        public int PageSize { get; set; }
    }
}
