using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class MediaType
    {
        public MediaType()
        {
            Media = new HashSet<Media>();
        }

        public byte Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Media> Media { get; set; }
    }
}
