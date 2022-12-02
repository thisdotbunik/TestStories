using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class ExperimentMedia
    {
        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public int ExperimentId { get; set; }
        public long MediaId { get; set; }
        public string TitleImage { get; set; }
        public int VideoPlayCount { get; set; }

        public virtual Experiment Experiment { get; set; }
        public virtual Media Media { get; set; }
    }
}
