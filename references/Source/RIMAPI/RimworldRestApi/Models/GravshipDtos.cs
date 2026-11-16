using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class GravshipMaintenanceRequest
    {
        public int? MapId { get; set; }
        public string EngineId { get; set; }
        public string TargetId { get; set; }
        public string PawnId { get; set; }

        /// <summary>
        /// "breakdown" (default, native FixBrokenDownBuilding) or "damage"
        /// (native WorkGiver_Repair for a damaged but not broken component).
        /// </summary>
        public string Mode { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class GravshipLaunchRequest
    {
        public int? MapId { get; set; }
        public string EngineId { get; set; }
        public string ConsoleId { get; set; }
        public string PilotId { get; set; }
        public string CopilotId { get; set; }
        public List<string> CrewIds { get; set; }
        public bool BoardColonyAnimals { get; set; }
        public bool BoardColonyMechs { get; set; }
        public bool ForceVisitorsToLeave { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class GravshipTakeoffRequest
    {
        public string RunId { get; set; }
        public int? LayerId { get; set; }
        public int? TileId { get; set; }
        public bool AllowAbandonMap { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class GravshipCancelRequest
    {
        public string RunId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class GravshipLandRequest
    {
        public string RunId { get; set; }
        public int? MapId { get; set; }
        // Same cell semantics as the native move-gravship designator.
        public int? X { get; set; }
        public int? Z { get; set; }
        public int? Rotation { get; set; }
        public bool DryRun { get; set; }
    }
}
