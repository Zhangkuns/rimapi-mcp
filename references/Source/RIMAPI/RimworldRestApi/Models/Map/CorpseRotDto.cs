using Newtonsoft.Json;

namespace RIMAPI.Models
{
    public class CorpseRotDto
    {
        public int MapId { get; set; }
        public int ThingId { get; set; }
        public string DefName { get; set; }
        public string Label { get; set; }
        public PositionDto Position { get; set; }
        public string RotStage { get; set; }
        public float RotProgress { get; set; }
        public float RotProgressPct { get; set; }
        public float Temperature { get; set; }
        public bool Active { get; set; }
        public bool Frozen { get; set; }
        public bool RotDestroys { get; set; }
        public int TicksToRotStart { get; set; }
        public int TicksToDessicated { get; set; }
        [JsonProperty("hours_until_rot", NullValueHandling = NullValueHandling.Include)]
        public int? HoursUntilRot { get; set; }

        [JsonProperty("hours_until_dessicated", NullValueHandling = NullValueHandling.Include)]
        public int? HoursUntilDessicated { get; set; }

        [JsonProperty("estimate_reason", NullValueHandling = NullValueHandling.Include)]
        public string EstimateReason { get; set; }
    }
}
