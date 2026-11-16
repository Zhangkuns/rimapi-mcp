// AI-FORK: DLC gene control DTOs (Biotech).
namespace RIMAPI.Models
{
    public class GeneActionRequest
    {
        public string PawnId { get; set; }
        public string GeneDef { get; set; }

        /// <summary>true targets the xenogene layer; false targets endogenes.</summary>
        public bool Xenogene { get; set; }

        public bool DryRun { get; set; }
    }

    public class XenotypeRequest
    {
        public string PawnId { get; set; }
        public string XenotypeDef { get; set; }

        /// <summary>Native SetXenotype rebuilds genes; the caller must confirm it.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
