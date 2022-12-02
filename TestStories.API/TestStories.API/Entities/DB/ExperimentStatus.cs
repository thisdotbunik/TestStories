using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class ExperimentStatus
    {
        public ExperimentStatus()
        {
            Experiment = new HashSet<Experiment>();
        }

        public byte Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Experiment> Experiment { get; set; }
    }
}
