using System;
using System.Collections.Generic;

namespace MillionStories.API.Entities.DB
{
    public partial class Experiment
    {
        public Experiment()
        {
            ExperimentMedia = new HashSet<ExperimentMedia>();
        }

        public byte[] Rv { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public byte ExperimenttypeId { get; set; }
        public byte ExperimentstatusId { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public string Goal { get; set; }
        public long? MediaId { get; set; }
        public int? VideoPlays { get; set; }
        public byte? EngagementtypeId { get; set; }

        public virtual User CreatedUser { get; set; }
        public virtual EngagementType Engagementtype { get; set; }
        public virtual ExperimentStatus Experimentstatus { get; set; }
        public virtual ExperimentType Experimenttype { get; set; }
        public virtual ICollection<ExperimentMedia> ExperimentMedia { get; set; }
    }
}
