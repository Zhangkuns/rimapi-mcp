using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class CaravanItemRequest
    {
        public string ThingId { get; set; }
        public int Count { get; set; } = 1;
    }

    public class CaravanCellRequest
    {
        public int X { get; set; }
        public int Z { get; set; }
    }

    public class CaravanFormingRequest
    {
        public int? MapId { get; set; }
        public string Action { get; set; }
        public int? LordId { get; set; }
        public List<string> PawnIds { get; set; }
        public List<string> AnimalIds { get; set; }
        public List<CaravanItemRequest> Items { get; set; }
        public List<CaravanItemRequest> TravelSupplies { get; set; }
        public int? DestinationTile { get; set; }
        public int? StartingTile { get; set; }
        public CaravanCellRequest MeetingPoint { get; set; }
        public bool? AutoTravelSupplies { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
