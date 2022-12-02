using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class SeriesViewModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "seriesTypeId")]
        public byte SeriesTypeId { get; set; }

        [JsonProperty(propertyName: "seriesName")]
        public string SeriesName { get; set; }

        [JsonProperty(propertyName: "title")]
        public string Title { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "showOnMenu")]
        public bool ShowOnMenu { get; set; }

        [JsonProperty(propertyName: "logoSize")]
        public int? LogoSize { get; set; }

        [JsonProperty(propertyName: "descriptionColor")]
        public string DescriptionColor { get; set; }
    }
}
