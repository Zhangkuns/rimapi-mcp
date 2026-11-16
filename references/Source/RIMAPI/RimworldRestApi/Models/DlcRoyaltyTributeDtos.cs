// AI-FORK: Royalty imperial tribute / royal-favor trade DTOs (RY-R05).
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class RoyalTributeSellLine
    {
        public int ThingId { get; set; }
        public int Count { get; set; }
    }

    public sealed class RoyalTributeRequest
    {
        public int MapId { get; set; }

        /// <summary>ThingID or numeric id of the favor trader pawn.</summary>
        public string TraderPawnId { get; set; }

        /// <summary>Free colonist whose royalty tracker receives the favor.</summary>
        public string NegotiatorPawnId { get; set; }

        /// <summary>Exact colony things and counts to hand over as tribute.</summary>
        public List<RoyalTributeSellLine> Sells { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
