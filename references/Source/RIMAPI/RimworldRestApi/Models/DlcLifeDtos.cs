// AI-FORK: DLC biotech life DTOs (G-35).
namespace RIMAPI.Models
{
    public class DeathrestStartRequest
    {
        public string PawnId { get; set; }
        public string BedId { get; set; }
        public bool DryRun { get; set; }
    }

    public class HemogenPolicyRequest
    {
        public string PawnId { get; set; }
        public float? TargetPercent { get; set; }
        public bool? PacksAllowed { get; set; }
        public bool DryRun { get; set; }
    }

    public class DeathrestAutoWakeRequest
    {
        public string PawnId { get; set; }
        public bool Enabled { get; set; }
        public bool DryRun { get; set; }
    }

    public class DeathrestWakeRequest
    {
        public string PawnId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    // G-52: carry a deathresting pawn to a casket.
    public class DeathrestCarryRequest
    {
        public string PawnId { get; set; }
        public string CarrierId { get; set; }
        public bool DryRun { get; set; }
    }

    // G-51: growth vat read/enter/cancel-load.
    public class GrowthVatEnterRequest
    {
        public int? MapId { get; set; }
        public string VatId { get; set; }
        public string PawnId { get; set; }

        /// <summary>Optional carrier for a downed/prisoner pawn.</summary>
        public string CarrierId { get; set; }

        public bool DryRun { get; set; }
    }

    public class GrowthVatEmbryoRequest
    {
        public int? MapId { get; set; }
        public string VatId { get; set; }
        public string EmbryoId { get; set; }
        public bool DryRun { get; set; }
    }

    public class GrowthVatCancelRequest
    {
        public int? MapId { get; set; }
        public string VatId { get; set; }
        /// <summary>
        /// Accepts the native growth-vat confirmation shown when an implanted
        /// embryo that has not started gestating is cancelled
        /// (Dialog_MessageBox "ImplantEmbryoCancelVat").
        /// </summary>
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    // G-50: normal prisoner bloodfeed job.
    public class HemogenFeedRequest
    {
        public int? MapId { get; set; }
        public string FeederId { get; set; }
        public string PrisonerId { get; set; }
        public bool DryRun { get; set; }
    }
}
