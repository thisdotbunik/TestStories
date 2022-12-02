using System;

namespace TestStories.API.Models.ResponseModels
{
    public class MediaSiteMap
    {
        public long Id { get; set; }

        public string SeoUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public string HlsUrl { get; set; }

        public string Thumbnail { get; set; }

        public DateTime? ExpireDate { get; set; }

        public DateTime? PublishDate { get; set; }

        public MediaMetaData MetaData { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
