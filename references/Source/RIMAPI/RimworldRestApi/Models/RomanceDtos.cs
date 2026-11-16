namespace RIMAPI.Models
{
    public sealed class RomanceRequest
    {
        public string InitiatorId { get; set; }
        public string TargetId { get; set; }
        public bool DryRun { get; set; }
    }
}
