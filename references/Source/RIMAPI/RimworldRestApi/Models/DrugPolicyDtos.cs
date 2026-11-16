namespace RIMAPI.Models
{
    public sealed class DrugPolicyCreateRequest
    {
        public string Label { get; set; }
        public int? CopyFromId { get; set; }
        public string PolicyDef { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class DrugPolicyIdRequest
    {
        public int PolicyId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class DrugPolicyEntryRequest
    {
        public int PolicyId { get; set; }
        public string DrugDef { get; set; }
        public bool? AllowedForAddiction { get; set; }
        public bool? AllowedForJoy { get; set; }
        public bool? AllowScheduled { get; set; }
        public float? DaysFrequency { get; set; }
        public float? OnlyIfMoodBelow { get; set; }
        public float? OnlyIfJoyBelow { get; set; }
        public int? TakeToInventory { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class PawnDrugPolicyRequest
    {
        public string PawnId { get; set; }
        public int PolicyId { get; set; }
        public bool DryRun { get; set; }
    }
}
