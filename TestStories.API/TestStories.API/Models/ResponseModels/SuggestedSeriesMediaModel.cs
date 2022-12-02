using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class SuggestedSeriesMediaModel
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }

        [JsonProperty(propertyName: "seriesTitle")]
        public string SeriesTitle { get; set; }

        [JsonProperty(propertyName: "seriesTypeId")]
        public int SeriesTypeId { get; set; }

        [JsonProperty(propertyName: "seriesType")]
        public string SeriesType { get; set; }

        [JsonProperty(propertyName: "seriesDescription")]
        public string SeriesDescription { get; set; }

        [JsonProperty(propertyName: "seriesLogo")]
        public string SeriesLogo { get; set; }

        [JsonProperty(propertyName: "seriesLogos")]
        public Images SeriesLogos { get; set; } = new Images();

        [JsonProperty(propertyName: "logoFileName")]
        public string LogoFileName { get; set; }

        [JsonProperty(propertyName: "videoThumbnail")]
        public string VideoThumbnail { get; set; }

        [JsonProperty(propertyName: "videoThumbnails")]
        public Images VideoThumbnails { get; set; } = new Images();

        [JsonProperty(propertyName: "videoLink")]
        public string VideoLink { get; set; }

        [JsonProperty(propertyName: "videoCount")]
        public int VideoCount { get; set; }
        public string StatusOnCloud { get; set; }

        [JsonProperty(propertyName: "seriesImage")]
        public string SeriesImage { get; set; }

        [JsonProperty(propertyName: "seriesImages")]
        public Images SeriesImages { get; set; } = new Images();

        [JsonProperty(propertyName: "imageFileName")]
        public string ImageFileName { get; set; }

        [JsonProperty(propertyName: "homepageBanner")]
        public string HomepageBanner { get; set; }

        [JsonProperty(propertyName: "homepageBanners")]
        public Images HomepageBanners { get; set; } = new Images();

        [JsonProperty(propertyName: "homepageBannerName")]
        public string HomepageBannerName { get; set; }

        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "mediaTypeId")]
        public int MediaTypeId { get; set; }

        [JsonProperty(propertyName: "suggestedMedias")]
        public List<MediaInfoModel> SuggestedMedias { get; set; }

        [JsonProperty(propertyName: "seoUrl")]
        public string SeoUrl { get; set; }

        [JsonProperty(propertyName: "medias")]
        public ICollection<DataAccess.Entities.Media> Medias { get; set; }
    }
}
