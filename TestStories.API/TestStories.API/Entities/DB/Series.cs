using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Series
    {
        public Series()
        {
            Media = new HashSet<Media>();
            SubscriptionSeries = new HashSet<SubscriptionSeries>();
            ToolSeries = new HashSet<ToolSeries>();
        }

        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string FeaturedImage { get; set; }
        public string LogoMetadata { get; set; }
        public string FeaturedImageMetadata { get; set; }
        public string HomepageBanner { get; set; }
        public string HomepageBannerMetadata { get; set; }

        public virtual ICollection<Media> Media { get; set; }
        public virtual ICollection<SubscriptionSeries> SubscriptionSeries { get; set; }
        public virtual ICollection<ToolSeries> ToolSeries { get; set; }
    }
}
