namespace RIMAPI.Models
{
    public class ActivityTargetRequest
    {
        public int? MapId { get; set; }
        public string TargetId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class ActivityPolicyRequest : ActivityTargetRequest
    {
        public bool? Enabled { get; set; }
        public float? SuppressIfAbove { get; set; }
    }

    public sealed class ActivitySuppressRequest : ActivityTargetRequest
    {
        public string PawnId { get; set; }
    }
}
