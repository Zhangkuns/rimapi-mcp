using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>S-08: native CompPlantable seed-cell selection and PlantSeed job.</summary>
    public sealed class PlantableRequestDto
    {
        public int SeedId { get; set; }
        public string Action { get; set; }
        public List<PositionDto> Cells { get; set; }
        public int? PawnId { get; set; }
        public PositionDto Cell { get; set; }
        public bool DryRun { get; set; }
    }
}
