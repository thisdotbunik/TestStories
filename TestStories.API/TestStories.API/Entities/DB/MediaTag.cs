using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class MediaTag
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public long MediaId { get; set; }
        public int TagId { get; set; }

        public virtual Media Media { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
