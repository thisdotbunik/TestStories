using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class AddSeriesModel
    {
        [JsonProperty(propertyName: "seriesTypeId")]
        [Required]
        [Range(1, 3, ErrorMessage = "SeriesTypeId must be between {0} and {1}")]
        public byte SeriesTypeId { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "seriesTitle")]
        [Required]
        public string SeriesTitle { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "seriesDescription")]
        public string SeriesDescription { get; set; }

        [JsonProperty(propertyName: "seriesLogo")]
        public IFormFile SeriesLogo { get; set; }

        [JsonProperty(propertyName: "seriesImage")]
        public IFormFile SeriesImage { get; set; }

        [JsonProperty(propertyName: "homepageBanner")]
        public IFormFile HomepageBanner { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "suggestedMediaIds")]
        public string SuggestedMediaIds { get; set; }

        [JsonProperty(propertyName: "showOnMenu")]
        public bool ShowOnMenu { get; set; }

        [DefaultValue(null)]
        [JsonProperty(propertyName: "logoSize")]
        public int? SeriesLogoSize { get; set; }

        [DefaultValue(null)]
        [JsonProperty(propertyName: "descriptionColor")]
        public string SeriesDescriptionColor { get; set; }
    }
}
