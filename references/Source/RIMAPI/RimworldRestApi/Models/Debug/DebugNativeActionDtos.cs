using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class DebugNativeActionRequest
    {
        public string Action { get; set; }
        public int? PawnId { get; set; }
        public int? IdeoId { get; set; }
        public string PreceptId { get; set; }
        public string DefName { get; set; }
        public int? Tile { get; set; }
        public int? Layer { get; set; }
        public float? Value { get; set; }
        public bool DryRun { get; set; }
        public int? ExpectedProcessId { get; set; }
        /// <summary>Existing construction ThingID or numeric thingIDNumber.</summary>
        public string ThingId { get; set; }
        public int? MapId { get; set; }
        public List<DebugRoofCell> Cells { get; set; }
        public DebugRoofCell PointA { get; set; }
        public DebugRoofCell PointB { get; set; }
    }

    public sealed class DebugRoofCell
    {
        public int? X { get; set; }
        public int? Z { get; set; }
    }
}
