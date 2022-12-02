using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class ToolSeries
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int ToolId { get; set; }
        public int SeriesId { get; set; }

        public virtual Series Series { get; set; }
        public virtual Tool Tool { get; set; }
    }
}
