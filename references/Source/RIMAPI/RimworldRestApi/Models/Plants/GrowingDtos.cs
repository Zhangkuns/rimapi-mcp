using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>C-22: mutate an existing Zone_Growing or Building_PlantGrower.</summary>
    public sealed class GrowingRequestDto
    {
        public int MapId { get; set; }
        public int? ZoneId { get; set; }
        public int? ThingId { get; set; }
        public string Action { get; set; }
        public string PlantDef { get; set; }
        public bool? AllowSow { get; set; }
        public bool? AllowCut { get; set; }
        public List<PositionDto> AddCells { get; set; }
        public List<PositionDto> RemoveCells { get; set; }
        public string Label { get; set; }
        public bool DryRun { get; set; }
    }
}
