namespace RIMAPI.Models
{
    /// <summary>RY-R09: normal ApplyTechprint at a research bench.</summary>
    public sealed class TechprintApplyRequest
    {
        public int? MapId { get; set; }
        public string PawnId { get; set; }
        public string TechprintId { get; set; }
        public string BenchId { get; set; }
        public bool DryRun { get; set; }
    }
}
