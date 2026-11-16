namespace RIMAPI.Models
{
    public sealed class ContainmentCaptureRequest
    {
        public int? MapId { get; set; }
        public int? PlatformId { get; set; }
        public string EntityId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }
}
