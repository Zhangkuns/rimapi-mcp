namespace RIMAPI.Models
{
    /// <summary>
    /// ID-R06: native CompNeuralSupercharger auto-use policy. Mode is one of
    /// NoAutoUse, AutoUseWithDesire, AutoUseForEveryone; AllowGuests is the
    /// separate native toggle.
    /// </summary>
    public sealed class SuperchargerPolicyRequest
    {
        public string ThingId { get; set; }
        public int? MapId { get; set; }
        public string AutoUseMode { get; set; }
        public bool? AllowGuests { get; set; }
        public bool DryRun { get; set; }
    }
}
