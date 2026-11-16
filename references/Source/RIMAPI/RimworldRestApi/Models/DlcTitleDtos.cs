// AI-FORK: DLC royalty title DTOs (G-28).
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class BestowingQuestRequest
    {
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class RoyalDecreeRequest
    {
        public string PawnId { get; set; }

        /// <summary>Required for a non-dry run: issuing a decree generates a quest.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class BestowingQuestAcceptRequest
    {
        public string PawnId { get; set; }

        /// <summary>Required for a non-dry run: accepting the quest starts the native quest flow.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class BestowingCeremonyCancelRequest
    {
        public string PawnId { get; set; }

        /// <summary>Required for a non-dry run: cancel ends the quest as failed.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class BestowingCeremonyStartRequest
    {
        public string PawnId { get; set; }

        /// <summary>Optional free-colonist participants to add before the ceremony memo.</summary>
        public List<string> ParticipantIds { get; set; }

        /// <summary>Required for a non-dry run: starting the ceremony changes lord/job state.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    public class TitleRelinquishRequest
    {
        public string PawnId { get; set; }
        public string FactionDef { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public class TitleGrantRequest
    {
        public string PawnId { get; set; }
        public string TitleDef { get; set; }

        /// <summary>Faction defName; defaults to the pawn's existing royal faction or Empire.</summary>
        public string FactionDef { get; set; }

        /// <summary>Native rewards/letter are opt-in; the default grants the title only.</summary>
        public bool GrantRewards { get; set; }

        /// <summary>Native SetTitle resets favor on a downgrade; this must be explicit.</summary>
        public bool AllowDowngrade { get; set; }

        public bool SendLetter { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
