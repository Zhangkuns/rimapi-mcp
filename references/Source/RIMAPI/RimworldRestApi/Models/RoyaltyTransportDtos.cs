// AI-FORK: G-29 Task C restricted transport boarding.
namespace RIMAPI.Models
{
    public class TransportShipActionRequest
    {
        public string ShipThingId { get; set; }
        /// <summary>send, dismiss or auto_load; only native enabled choices are accepted.</summary>
        public string Action { get; set; }
        public bool? AutoLoad { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public class TransportBoardRequest
    {
        /// <summary>Player-controlled free colonist to send into the transport.</summary>
        public string PawnId { get; set; }

        /// <summary>TransportShip load id (string form of TransportShip.loadID).</summary>
        public string TransportShipId { get; set; }

        /// <summary>Optional ship ThingID; used when the load id is not supplied.</summary>
        public string ShipThingId { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}

// AI-FORK: G-29 Task C carrying a downed pawn to a transport with the native haul job.
namespace RIMAPI.Models
{
    public class TransportCarryRequest
    {
        /// <summary>Player-controlled free colonist who performs the carry.</summary>
        public string ActorPawnId { get; set; }

        /// <summary>Downed pawn to carry into the transport.</summary>
        public string TargetPawnId { get; set; }

        public string TransportShipId { get; set; }
        public string ShipThingId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}

// AI-FORK: G-29 formal restricted cancellation.
namespace RIMAPI.Models
{
    public class CarryCancelRequest
    {
        public string ActorPawnId { get; set; }
        public string TargetPawnId { get; set; }
        public string ShipThingId { get; set; }
        public int ExpectedJobId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public class LoadingCancelRequest
    {
        public string ShipThingId { get; set; }
        public int? ExpectedGroupId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}

// AI-FORK: G-29 restricted permit-shuttle travel submission.
namespace RIMAPI.Models
{
    public class TransportTravelRequest
    {
        public string ShipThingId { get; set; }
        public int? DestinationTile { get; set; }
        /// <summary>attack or visit; default depends on settlement hostility.</summary>
        public string ArrivalAction { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
