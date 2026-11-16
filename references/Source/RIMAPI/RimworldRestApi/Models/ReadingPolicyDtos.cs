namespace RIMAPI.Models
{
    public sealed class ReadingPolicyCreateRequest
    {
        public string Label { get; set; }
        public int? CopyFromId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class ReadingPolicyIdRequest
    {
        public int PolicyId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class ReadingPolicyFilterRequest
    {
        public int PolicyId { get; set; }
        public string ThingDef { get; set; }
        public string SpecialFilter { get; set; }
        public bool Allowed { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class PawnReadingPolicyRequest
    {
        public string PawnId { get; set; }
        public int PolicyId { get; set; }
        public bool DryRun { get; set; }
    }
}
