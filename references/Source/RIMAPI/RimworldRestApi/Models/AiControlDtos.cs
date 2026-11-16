// AI-FORK: request DTOs for the Phase 1b control endpoints (G-01..G-04).
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class DesignateRequestV2
    {
        public int MapId { get; set; }

        /// <summary>Any DesignationDef defName, e.g. "Mine", "Haul", "Hunt".</summary>
        public string DesignationDef { get; set; }

        public List<PositionDto> Cells { get; set; }
        public List<int> ThingIds { get; set; }

        /// <summary>Optional rectangle, inclusive, in addition to Cells.</summary>
        public PositionDto PointA { get; set; }
        public PositionDto PointB { get; set; }

        public bool Remove { get; set; }
        public bool DryRun { get; set; }
    }

    public class HediffRequest
    {
        public string PawnId { get; set; }
        public string HediffDef { get; set; }

        /// <summary>Optional body part defName, e.g. "LeftArm".</summary>
        public string BodyPart { get; set; }

        /// <summary>Negative keeps the def's default severity.</summary>
        public float Severity { get; set; } = -1f;

        public int Count { get; set; } = 1;
        public bool DryRun { get; set; }
    }

    public class HediffRemoveRequest
    {
        public string PawnId { get; set; }
        public string HediffDef { get; set; }
        public string BodyPart { get; set; }
        public bool All { get; set; }
        public bool DryRun { get; set; }
    }

    public class HediffSeverityRequest
    {
        public string PawnId { get; set; }
        public string HediffDef { get; set; }
        public string BodyPart { get; set; }
        public float Severity { get; set; }
        public bool DryRun { get; set; }
    }

    public class MentalStateRequest
    {
        public string PawnId { get; set; }
        public string StateDef { get; set; }
        public string Reason { get; set; }
        public bool Force { get; set; }
        public bool DryRun { get; set; }
    }

    public class InspirationRequest
    {
        public string PawnId { get; set; }
        public string InspirationDef { get; set; }
        public string Reason { get; set; }
        public bool DryRun { get; set; }
    }

    public class PawnIdRequest
    {
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public class PawnOrderRequest
    {
        public string PawnId { get; set; }

        /// <summary>goto | attack | attack_static</summary>
        public string Order { get; set; }

        public PositionDto Cell { get; set; }
        public int? ThingId { get; set; }
        public string TargetPawnId { get; set; }
        public bool DryRun { get; set; }
    }
}
