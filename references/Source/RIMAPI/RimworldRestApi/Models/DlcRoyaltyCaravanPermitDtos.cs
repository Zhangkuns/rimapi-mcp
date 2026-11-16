// AI-FORK: Royalty caravan permit DTOs (RY-R04).
namespace RIMAPI.Models
{
    public sealed class RoyalCaravanPermitRequest
    {
        /// <summary>ThingID of the royal pawn travelling in a player caravan.</summary>
        public string PawnId { get; set; }

        /// <summary>RoyalTitlePermitDef defName; must be a world-map permit.</summary>
        public string PermitDef { get; set; }

        /// <summary>World tile for CallTransportShuttle; the caravan tile is used when omitted.</summary>
        public int? TargetTile { get; set; }

        /// <summary>Zero-based native arrival option for the target tile; required when ambiguous.</summary>
        public int? ArrivalOptionIndex { get; set; }

        /// <summary>A drop that exceeds caravan mass capacity needs this explicit confirmation.</summary>
        public bool ConfirmOverweight { get; set; }

        /// <summary>Required for a non-dry run: using a permit spends a use or favor.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
