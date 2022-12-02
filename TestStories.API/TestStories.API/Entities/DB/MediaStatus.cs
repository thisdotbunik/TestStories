using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class MediaStatus
    {
        public MediaStatus()
        {
            Media = new HashSet<Media>();
        }

        public byte Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Media> Media { get; set; }
    }
}
