using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class SeriesMediaModel
    {
        [JsonProperty(propertyName: "seriesId")]
        public int SeriesId { get; set; }

        [JsonProperty(propertyName:"title")]
        public string Title { get; set; }

        [JsonProperty(propertyName:"description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }

        [JsonProperty(propertyName: "logos")]
        public Images Logos { get; set; } = new Images();

        [JsonProperty(propertyName: "featuredImage")]
        public string FeaturedImage { get; set; }

        [JsonProperty(propertyName: "featuredImages")]
        public Images FeaturedImages { get; set; } = new Images();

        [JsonProperty(propertyName: "videoCount")]
        public int VideoCount { get; set; }

        [JsonProperty(propertyName:"videos")]
        public List<MediaInfoModel> Videos { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "suggestedMedias")]
        public List<MediaInfoModel> SuggestedMedias { get; set; }

        [JsonProperty(propertyName: "logoSize")]
        public int? LogoSize { get; set; }

        [JsonProperty(propertyName: "descriptionColor")]
        public string DescriptionColor { get; set; }

    }
}
