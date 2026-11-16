namespace RIMAPI.Models
{
    public sealed class FacilitySettingsRequest
    {
        public string ThingId { get; set; }
        public bool? HoldOpen { get; set; }
        public bool? GatherSpotActive { get; set; }
        public string GlowColorHex { get; set; }
        public bool? Darklight { get; set; }
        public bool? TrapAutoRearm { get; set; }
        public bool? FirefoamAutoRebuild { get; set; }
        public bool? AutoCut { get; set; }
        public bool? AutoCutNow { get; set; }
        public string TargetMineable { get; set; }
        public bool DryRun { get; set; }
    }
}
