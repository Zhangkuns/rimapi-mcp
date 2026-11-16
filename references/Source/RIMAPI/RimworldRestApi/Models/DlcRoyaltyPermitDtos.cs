// AI-FORK: G-29 permit purchase/refund DTOs.
namespace RIMAPI.Models
{
    public class PermitPurchaseRequest
    {
        public string PawnId { get; set; }

        /// <summary>RoyalTitlePermitDef defName; its cost and faction come from the Def.</summary>
        public string Def { get; set; }

        /// <summary>Optional; when supplied it must match the def's own faction.</summary>
        public string FactionDef { get; set; }

        /// <summary>A purchase spends derived permit points; confirm it.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    public class PermitRefundRequest
    {
        public string PawnId { get; set; }

        /// <summary>Optional; defaults to Empire (the only faction with royal permits).</summary>
        public string FactionDef { get; set; }

        /// <summary>Refunding removes every permit of the faction and charges favor.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
