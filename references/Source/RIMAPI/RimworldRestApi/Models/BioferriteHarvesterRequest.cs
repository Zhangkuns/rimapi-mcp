using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class BioferriteHarvesterRequest
    {
        public int? MapId { get; set; }
        public string HarvesterId { get; set; }
        public string Action { get; set; }
        public bool DryRun { get; set; }
    }
}
