using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class PlaylistMedia
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public long MediaId { get; set; }
        public int MediaSequence { get; set; }

        public virtual Media Media { get; set; }
        public virtual Playlist Playlist { get; set; }
    }
}
