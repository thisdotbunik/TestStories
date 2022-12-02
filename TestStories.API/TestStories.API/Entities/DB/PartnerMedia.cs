using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class PartnerMedia
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public long MediaId { get; set; }
        public string Email { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public bool IsExpired { get; set; }

        public virtual Media Media { get; set; }
        public virtual Partner Partner { get; set; }
    }
}
