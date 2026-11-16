namespace RIMAPI.Models
{
    /// <summary>BT-R11: native CompAtomizer.AutoLoad and EjectContents.</summary>
    public sealed class AtomizerPolicyRequest
    {
        public int? MapId { get; set; }
        public string AtomizerId { get; set; }
        public bool? AutoLoad { get; set; }
        public bool Eject { get; set; }
        public bool DryRun { get; set; }
    }
}
