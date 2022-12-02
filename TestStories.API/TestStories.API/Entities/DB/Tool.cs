using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Tool
    {
        public Tool()
        {
            ToolMedia = new HashSet<ToolMedia>();
            ToolSeries = new HashSet<ToolSeries>();
        }

        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTime DateCreatedUtc { get; set; }

        public virtual ICollection<ToolMedia> ToolMedia { get; set; }
        public virtual ICollection<ToolSeries> ToolSeries { get; set; }
    }
}
