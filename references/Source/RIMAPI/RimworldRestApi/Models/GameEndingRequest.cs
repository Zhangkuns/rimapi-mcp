using Newtonsoft.Json;

namespace RIMAPI.Models
{
    public class GameEndingRequest
    {
        [JsonProperty("confirm")]
        public bool Confirm { get; set; }
        [JsonProperty("dry_run")]
        public bool DryRun { get; set; }
    }
}
