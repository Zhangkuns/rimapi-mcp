// AI-FORK: Royalty heir request/change DTOs (RY-R03).
namespace RIMAPI.Models
{
    public sealed class RoyalHeirRequest
    {
        /// <summary>ThingID of the colonist who holds the inheritable title.</summary>
        public string PawnId { get; set; }

        /// <summary>ThingID of the selected candidate from the native heir list.</summary>
        public string HeirId { get; set; }

        /// <summary>Optional faction defName; defaults to the pawn's title faction.</summary>
        public string FactionDef { get; set; }

        /// <summary>Required for a non-dry run: requesting an heir starts a native quest.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
