using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Playlist
    {
        public Playlist()
        {
            PlaylistMedia = new HashSet<PlaylistMedia>();
        }

        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<PlaylistMedia> PlaylistMedia { get; set; }
    }
}
