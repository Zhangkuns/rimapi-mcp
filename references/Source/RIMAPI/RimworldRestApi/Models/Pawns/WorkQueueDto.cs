using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>
    /// C-14: normal player work-giver/clean/fire/fetilize/priority actions. The
    /// route intentionally exposes only named native actions, not arbitrary jobs.
    /// </summary>
    public class WorkQueueRequestDto
    {
        public int PawnId { get; set; }
        public string Action { get; set; }
        public PositionDto Cell { get; set; }
        public int? ThingId { get; set; }
        public string WorkGiver { get; set; }
        public bool? Enabled { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
