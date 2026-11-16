// AI-FORK: OD-R09 locked ancient door/box legal unlock and relay readback.
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class LockedActionRequest
    {
        public int? MapId { get; set; }

        /// <summary>Exact spawned ThingID (string form, e.g. "AncientBlastDoor75478").</summary>
        public string ThingId { get; set; }

        /// <summary>Player pawn that receives the native ordered job.</summary>
        public string PawnId { get; set; }

        /// <summary>open (IOpenable via JobDefOf.Open) or force_open (jammed door).</summary>
        public string Action { get; set; }

        public bool DryRun { get; set; }

        public bool Confirm { get; set; }
    }
}
