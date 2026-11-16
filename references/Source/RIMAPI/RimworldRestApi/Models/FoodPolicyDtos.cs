namespace RIMAPI.Models
{
    public sealed class FoodPolicyCreateRequest
    {
        public string Label { get; set; }
        public int? CopyFromId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class FoodPolicyIdRequest
    {
        public int PolicyId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class FoodPolicyFilterRequest
    {
        public int PolicyId { get; set; }
        public string ThingDef { get; set; }
        public string SpecialFilter { get; set; }
        public bool Allowed { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class PawnFoodPolicyRequest
    {
        public string PawnId { get; set; }
        public int PolicyId { get; set; }
        public bool DryRun { get; set; }
    }
}
