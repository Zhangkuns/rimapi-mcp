using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class TransportPodItemRequest
    {
        public string ThingId { get; set; }
        public int Count { get; set; } = 1;
    }

    public class TransportPodRequest
    {
        public string PodId { get; set; }
        public int? MapId { get; set; }
        public string Action { get; set; }
        public List<string> PawnIds { get; set; }
        public List<TransportPodItemRequest> Items { get; set; }
        public bool AutoLoad { get; set; }
        public int? TargetTile { get; set; }
        public string Arrival { get; set; }
        public bool Confirm { get; set; }
        public bool ConfirmSpace { get; set; }
        public bool DryRun { get; set; }
    }
}
