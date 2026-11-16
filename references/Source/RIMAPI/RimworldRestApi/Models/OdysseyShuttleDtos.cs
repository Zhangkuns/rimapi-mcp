// AI-FORK: Odyssey OD-R06 passenger shuttle map/caravan load, refuel and launch.
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class OdysseyShuttleItemRequest
    {
        /// <summary>Exact spawned ThingIDs making up one native TransferableOneWay.</summary>
        public List<string> ThingIds { get; set; }

        /// <summary>Requested transfer count; must be between 1 and the sum of stack counts.</summary>
        public int Count { get; set; }
    }

    public sealed class OdysseyShuttleRequest
    {
        /// <summary>Map shuttle ThingID. Exactly one of ShuttleId/CaravanId is required.</summary>
        public string ShuttleId { get; set; }

        /// <summary>World caravan holding a passenger shuttle.</summary>
        public int? CaravanId { get; set; }

        /// <summary>load, cancel_load, refuel, arrival_options or launch.</summary>
        public string Action { get; set; }

        public List<string> PawnIds { get; set; }

        public List<OdysseyShuttleItemRequest> Items { get; set; }

        /// <summary>Refuel amount; omitted consumes as much cargo fuel as fits.</summary>
        public int? RefuelCount { get; set; }

        /// <summary>World tile id; defaults to the surface layer.</summary>
        public int? DestinationTile { get; set; }

        /// <summary>A discovered arrival key; required when more than one choice exists.</summary>
        public string Arrival { get; set; }

        public int? DestinationWorldObjectId { get; set; }

        public int? LandingX { get; set; }
        public int? LandingZ { get; set; }
        /// <summary>North=0, East=1, South=2, West=3; defaults to native placing rotation.</summary>
        public int? LandingRotation { get; set; }
        public bool ConfirmSpace { get; set; }

        /// <summary>Required when the native tile offers more than one arrival option.</summary>
        public int? ArrivalOptionIndex { get; set; }

        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
