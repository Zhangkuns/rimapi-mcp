using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>
    /// OD-06a: normal Zone_Fishing creation/expansion. Cells must satisfy the native
    /// designator checks (water body with fish, passable terrain, no other zone).
    /// </summary>
    public sealed class FishingZoneRequest
    {
        public int MapId { get; set; }
        public PositionDto PointA { get; set; }
        public PositionDto PointB { get; set; }
        public List<PositionDto> Cells { get; set; }
        public string Label { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>
    /// OD-06b: the fishing policy fields the native ITab_Fishing writes.
    /// </summary>
    public sealed class FishingZoneDeleteRequest
    {
        public int MapId { get; set; }
        public int ZoneId { get; set; }
    }

    public sealed class FishingZonePolicyRequest
    {
        public int MapId { get; set; }
        public int ZoneId { get; set; }
        public bool? Allowed { get; set; }
        public string RepeatMode { get; set; }
        public int? RepeatCount { get; set; }
        public int? TargetCount { get; set; }
        public float? TargetPopulationPct { get; set; }
        public bool? PauseWhenSatisfied { get; set; }
        public int? UnpauseAtCount { get; set; }
        public bool DryRun { get; set; }
    }
}
