// AI-FORK: DLC royalty permit execution DTO (G-29b).
namespace RIMAPI.Models
{
    public class PermitUseRequest
    {
        public string PawnId { get; set; }
        public string PermitDef { get; set; }
        public PositionDto Cell { get; set; }

        /// <summary>Native delivery spends a favor charge or a free use; confirm it.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
