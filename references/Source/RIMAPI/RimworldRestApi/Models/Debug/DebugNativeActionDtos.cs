namespace RIMAPI.Models
{
    public sealed class DebugNativeActionRequest
    {
        public string Action { get; set; }
        public int? PawnId { get; set; }
        public int? IdeoId { get; set; }
        public string PreceptId { get; set; }
        public string DefName { get; set; }
        public int? Tile { get; set; }
        public int? Layer { get; set; }
        public float? Value { get; set; }
        public bool DryRun { get; set; }
        public int? ExpectedProcessId { get; set; }
    }
}
