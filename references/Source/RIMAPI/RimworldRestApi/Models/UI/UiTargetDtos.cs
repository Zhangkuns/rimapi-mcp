using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class UiTargetRequest
    {
        public string Kind { get; set; }
        public int? MapId { get; set; }
        public int? ThingId { get; set; }
        public int? X { get; set; }
        public int? Z { get; set; }
        public int? Tile { get; set; }
        public int? Layer { get; set; }
        public int? WorldObjectId { get; set; }
        public int? ExpectedProcessId { get; set; }
    }

    public sealed class UiTargetIdentityDto
    {
        public int? MapId { get; set; }
        public int? ThingId { get; set; }
        public int? CellX { get; set; }
        public int? CellZ { get; set; }
        public int? Tile { get; set; }
        public int? Layer { get; set; }
        public int? WorldObjectId { get; set; }
    }

    public sealed class UiTargetResolvedDto
    {
        public int ProcessId { get; set; }
        public int Frame { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public float UiScale { get; set; }
        public string Kind { get; set; }
        public string Label { get; set; }
        public UiTargetIdentityDto Target { get; set; }
        public string ContextId { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public bool Visible { get; set; }
        public bool Clickable { get; set; }
        public string Reason { get; set; }
        public bool MapTargeting { get; set; }
        public bool WorldTargeting { get; set; }
        // A click may open a selection menu when more than one object overlaps.
        public List<int> CandidateThingIds { get; set; } = new List<int>();
        public List<int> CandidateWorldObjectIds { get; set; } = new List<int>();
    }

    public sealed class UiTargetFocusDto
    {
        public int ProcessId { get; set; }
        public string Kind { get; set; }
        public UiTargetIdentityDto Target { get; set; }
        public string Outcome { get; set; } = "camera_focus_requested; resolve again after the next frame";
    }
}
