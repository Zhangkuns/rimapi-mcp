// AI-FORK: Odyssey OD-R05 MapPortal load/wait/cancel DTOs.
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class OdysseyPortalItemRequest
    {
        /// <summary>Exact spawned ThingIDs making up one native TransferableOneWay.</summary>
        public List<string> ThingIds { get; set; }

        /// <summary>Requested transfer count; must be between 1 and the sum of stack counts.</summary>
        public int Count { get; set; }
    }

    public sealed class OdysseyPortalRequest
    {
        public string PortalId { get; set; }

        /// <summary>load replaces the native leftToLoad list; set_count edits one row; cancel clears it.</summary>
        public string Action { get; set; }

        public List<string> PawnIds { get; set; }

        /// <summary>Single pawn for enter/seal; PawnIds remains the load selection.</summary>
        public string PawnId { get; set; }

        public List<OdysseyPortalItemRequest> Items { get; set; }

        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
