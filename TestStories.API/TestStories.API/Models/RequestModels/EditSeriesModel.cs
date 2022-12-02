using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TestStories.API.Models.RequestModels
{
    public class EditSeriesModel
    {
        [JsonProperty(propertyName: "id")]
        [Required]
        public int Id { get; set; }

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

        [DefaultValue("")]
        [JsonProperty(propertyName: "logoFileName")]
        public string LogoFileName { get; set; }

        [JsonProperty(propertyName: "seriesImage")]
        public IFormFile SeriesImage { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "imageFileName")]
        public string ImageFileName { get; set; }

        [JsonProperty(propertyName: "homepageBanner")]
        public IFormFile HomepageBanner { get; set; }

        [DefaultValue("")]
        [JsonProperty(propertyName: "homepageBannerName")]
        public string HomepageBannerName { get; set; }

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
