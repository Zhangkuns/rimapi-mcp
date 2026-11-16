using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class UtilityVerbRequest
    {
        public int? PawnId { get; set; }
        public string VerbId { get; set; }
        public string TargetType { get; set; }
        public string TargetId { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? Z { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
