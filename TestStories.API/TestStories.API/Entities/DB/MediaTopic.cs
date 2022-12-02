using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class MediaTopic
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public long MediaId { get; set; }
        public int TopicId { get; set; }

        public virtual Media Media { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
