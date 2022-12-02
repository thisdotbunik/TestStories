using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class SeoTagModel
    {
        [Required]
        [JsonProperty(propertyName: "entityId")]
        public long EntityId { get; set; }

        [Required]
        [JsonProperty(propertyName: "entityTypeId")]
        public byte EntityTypeId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "PrimaryKeyword")]
        public string PrimaryKeyword { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "secondaryKeyword")]
        public string SecondaryKeyword { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "titleTag")]
        public string TitleTag { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "metaDescription")]
        public string MetaDescription { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "pageDescription")]
        public string PageDescription { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "h1")]
        public string H1 { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "h2")]
        public string H2 { get; set; }
    }
}
