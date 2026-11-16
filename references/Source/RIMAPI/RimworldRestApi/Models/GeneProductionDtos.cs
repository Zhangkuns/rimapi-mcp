// AI-FORK: Biotech normal gene production (gene extractor -> genepack -> xenogerm).
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class GeneExtractCancelRequest
    {
        public int? MapId { get; set; }
        public string ExtractorId { get; set; }
        public bool DryRun { get; set; }
    }

    public class GeneExtractRequest
    {
        public int? MapId { get; set; }
        public string ExtractorId { get; set; }
        public string PawnId { get; set; }

        /// <summary>
        /// Optional colonist who performs the native CarryToBuilding job. A downed
        /// donor or a prisoner cannot enter by themselves, so an explicit carrier is
        /// required for them; omitted means the nearest capable colonist is used.
        /// </summary>
        public string CarrierId { get; set; }

        /// <summary>Native warning confirmation for a replicating xenogerm donor.</summary>
        public bool ConfirmReplicating { get; set; }

        public bool DryRun { get; set; }
    }

    /// <summary>
    /// BT-R19: native xenogerm reimplantation, i.e. another pawn absorbs the
    /// xenogenes of a downed or secure-prisoner donor (XenogermReimplanter gene).
    /// </summary>
    public class GeneReimplantRequest
    {
        public int? MapId { get; set; }
        public string DonorId { get; set; }
        public string AbsorberId { get; set; }

        /// <summary>
        /// Explicit confirmation for the native warning shown when the donor has a
        /// replicating xenogerm (the native dialog marks the action destructive).
        /// </summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    public class GeneAssembleRequest
    {
        public int? MapId { get; set; }
        public string AssemblerId { get; set; }
        public List<string> GenepackIds { get; set; }
        public string XenotypeName { get; set; }

        /// <summary>Optional native XenotypeIconDef defName for the finished xenogerm.</summary>
        public string IconDef { get; set; }

        /// <summary>Optional colonist to take the native CreateXenogerm job.</summary>
        public string WorkerId { get; set; }

        public bool DryRun { get; set; }
    }
}

namespace RIMAPI.Models
{
    /// <summary>BT-11b: select an ovum and its father for the native FertilizeOvum job.</summary>
    public sealed class FertilizeOvumRequest
    {
        public int? MapId { get; set; }
        public string OvumId { get; set; }
        public string FatherId { get; set; }
        public bool ConfirmInbred { get; set; }
        public bool DryRun { get; set; }
    }
}
