using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Tag
    {
        public Tag()
        {
            MediaTag = new HashSet<MediaTag>();
        }

        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<MediaTag> MediaTag { get; set; }
    }
}
