namespace RIMAPI.Models
{
    public sealed class GuestPolicyActionRequest
    {
        public string PawnId { get; set; }
        public string Action { get; set; }
        public string Exclusive { get; set; }
        public string Toggle { get; set; }
        public bool? ToggleEnabled { get; set; }
        public string SlaveMode { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
