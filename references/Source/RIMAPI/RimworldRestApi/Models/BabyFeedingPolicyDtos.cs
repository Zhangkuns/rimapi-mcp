namespace RIMAPI.Models
{
    /// <summary>BT-R08: autofeeder mode and allowed baby food.</summary>
    public sealed class BabyFeedingPolicyRequest
    {
        public string BabyId { get; set; }
        public string FeederId { get; set; }
        /// <summary>Never | Childcare | Urgent</summary>
        public string Mode { get; set; }
        public string FoodDef { get; set; }
        public bool? Allowed { get; set; }
        public bool DryRun { get; set; }
    }
}
