using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class MediaSrt
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string File { get; set; }
        public string FileMetadata { get; set; }
        public string Language { get; set; }
        public long MediaId { get; set; }

        public virtual Media Media { get; set; }
    }
}
