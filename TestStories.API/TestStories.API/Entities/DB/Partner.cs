using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Partner
    {
        public Partner()
        {
            Media = new HashSet<Media>();
            PartnerMedia = new HashSet<PartnerMedia>();
            PartnerPartnerType = new HashSet<PartnerPartnerType>();
        }

        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public bool ShowOnPartnerPage { get; set; }
        public bool IsArchived { get; set; }
        public DateTime DateAddedUtc { get; set; }
        public string LogoMetadata { get; set; }
        public string Link { get; set; }

        public virtual ICollection<Media> Media { get; set; }
        public virtual ICollection<PartnerMedia> PartnerMedia { get; set; }
        public virtual ICollection<PartnerPartnerType> PartnerPartnerType { get; set; }
    }
}
