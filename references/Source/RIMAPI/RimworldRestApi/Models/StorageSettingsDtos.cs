namespace RIMAPI.Models
{
    public sealed class StorageSettingsRequest
    {
        public string ThingId { get; set; }
        public string Priority { get; set; }
        public string FilterDef { get; set; }
        public bool? FilterAllowed { get; set; }
        public string GroupLabel { get; set; }
        public bool? CreateGroup { get; set; }
        public bool? EnrollGroup { get; set; }
        public int? GroupId { get; set; }
        public bool? LeaveGroup { get; set; }
        public string SpecialFilter { get; set; }
        public bool? SpecialFilterAllowed { get; set; }
        public float? MinHitPointsPercent { get; set; }
        public float? MaxHitPointsPercent { get; set; }
        public string MinQuality { get; set; }
        public string MaxQuality { get; set; }
        public bool DryRun { get; set; }
    }
}
