using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class ToolMedia
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int ToolId { get; set; }
        public long MediaId { get; set; }

        public virtual Media Media { get; set; }
        public virtual Tool Tool { get; set; }
    }
}
