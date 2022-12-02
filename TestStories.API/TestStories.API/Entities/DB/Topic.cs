using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Topic
    {
        public Topic()
        {
            InverseParent = new HashSet<Topic>();
            Media = new HashSet<Media>();
            MediaTopic = new HashSet<MediaTopic>();
            SubscriptionTopic = new HashSet<SubscriptionTopic>();
        }

        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string LogoMetadata { get; set; }

        public virtual Topic Parent { get; set; }
        public virtual ICollection<Topic> InverseParent { get; set; }
        public virtual ICollection<Media> Media { get; set; }
        public virtual ICollection<MediaTopic> MediaTopic { get; set; }
        public virtual ICollection<SubscriptionTopic> SubscriptionTopic { get; set; }
    }
}
