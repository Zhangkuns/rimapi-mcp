namespace RIMAPI.Models
{
    /// <summary>C-25: native CompFlickable want/switch and power reconnect.</summary>
    public sealed class PowerSwitchRequest
    {
        public string ThingId { get; set; }
        public bool? DesiredOn { get; set; }
        public bool Cancel { get; set; }
        public bool Reconnect { get; set; }
        public bool DryRun { get; set; }
    }
}
