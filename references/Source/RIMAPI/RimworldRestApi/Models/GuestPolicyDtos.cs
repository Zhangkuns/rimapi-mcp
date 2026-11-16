namespace RIMAPI.Models
{
    public sealed class GuestPolicyRequest
    {
        public string PawnId { get; set; }
        public string Exclusive { get; set; }
        public string Toggle { get; set; }
        public bool? ToggleEnabled { get; set; }
        public string SlaveMode { get; set; }
        public bool DryRun { get; set; }
    }
}
