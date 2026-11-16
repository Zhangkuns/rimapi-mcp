// AI-FORK: G-29 worker execution DTOs (CallAid / CallLaborers / CallShuttle / OrbitalStrike).
using RIMAPI.Models;

namespace RIMAPI.Models
{
    public class PermitExecuteRequest
    {
        public string PawnId { get; set; }

        /// <summary>RoyalTitlePermitDef defName; the Def's worker decides the native chain.</summary>
        public string Def { get; set; }

        /// <summary>Target cell; every supported worker requires a map target.</summary>
        public PositionDto Cell { get; set; }

        /// <summary>Required for execution: the native action may open a confirmation dialog.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
