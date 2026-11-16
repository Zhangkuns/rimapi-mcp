namespace RIMAPI.Models
{
    public sealed class OutfitPolicyCreateRequest
    {
        public string Label { get; set; }
        public int? CopyFromId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class OutfitPolicyIdRequest
    {
        public int OutfitId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class OutfitPolicyFilterRequest
    {
        public int OutfitId { get; set; }
        public string ThingDef { get; set; }
        public string SpecialFilter { get; set; }
        public float? MinHitPointsPercent { get; set; }
        public float? MaxHitPointsPercent { get; set; }
        public string MinQuality { get; set; }
        public string MaxQuality { get; set; }
        public bool Allowed { get; set; }
        public bool DryRun { get; set; }
    }
}
