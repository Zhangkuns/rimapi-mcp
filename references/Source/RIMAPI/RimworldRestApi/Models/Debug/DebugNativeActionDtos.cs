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
        /// <summary>Existing target ThingID or numeric thingIDNumber.</summary>
        public string ThingId { get; set; }
        public int? MapId { get; set; }
        public int? X { get; set; }
        public int? Z { get; set; }
        public List<DebugRoofCell> Cells { get; set; }
        public DebugRoofCell PointA { get; set; }
        public DebugRoofCell PointB { get; set; }
        public ItemTransferLocation Source { get; set; }
        public ItemTransferLocation Destination { get; set; }
        public int? StackCapacity { get; set; }
        /// <summary>Existing bill identity on a workbench; alternative to bill_index.</summary>
        public string BillId { get; set; }
        /// <summary>Zero-based index in the workbench bill stack; alternative to bill_id.</summary>
        public int? BillIndex { get; set; }
        /// <summary>Optional expected recipe defName; rejected when it does not match the selected bill.</summary>
        public string RecipeDefName { get; set; }
        /// <summary>Optional explicit ingredient ThingIDs; omitted means native ingredient selection.</summary>
        public List<string> Ingredients { get; set; }
        /// <summary>Optional stage selector for multi-stage devices (for example add/finish/take).</summary>
        public string Mode { get; set; }
        /// <summary>Force a deterministic success where the action documents a debug-only bypass.</summary>
        public bool Force { get; set; }
    }

    public sealed class ItemTransferLocation
    {
        public string Kind { get; set; }
        public string ThingId { get; set; }
        public int? ZoneId { get; set; }
        public int? X { get; set; }
        public int? Z { get; set; }
    }

    public sealed class DebugRoofCell
    {
        public int? X { get; set; }
        public int? Z { get; set; }
    }
}
