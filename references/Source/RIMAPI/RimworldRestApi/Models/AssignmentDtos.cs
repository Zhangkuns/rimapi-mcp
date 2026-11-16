namespace RIMAPI.Models
{
    public sealed class AssignableRequest
    {
        public string ThingId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class BedSettingsRequest
    {
        public string ThingId { get; set; }
        public bool? ForPrisoners { get; set; }
        public bool? Medical { get; set; }
        public bool DryRun { get; set; }
    }
}
