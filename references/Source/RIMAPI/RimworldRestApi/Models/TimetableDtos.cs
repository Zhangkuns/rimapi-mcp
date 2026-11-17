using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class TimetableBatchRequest
    {
        public List<string> PawnIds { get; set; }
        public bool AllColonists { get; set; }
        public int? MapId { get; set; }
        public List<TimetableEdit> Edits { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class TimetableEdit
    {
        public string Assignment { get; set; }
        public bool AllDay { get; set; }
        public int? StartHour { get; set; }
        public int? EndHour { get; set; }
        public List<int> Hours { get; set; }
        public List<string> Assignments { get; set; }
    }
}
