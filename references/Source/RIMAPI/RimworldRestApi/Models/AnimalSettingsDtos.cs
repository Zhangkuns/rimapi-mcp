namespace RIMAPI.Models
{
    public sealed class AnimalSettingsRequest
    {
        public string AnimalId { get; set; }
        public string MasterId { get; set; }
        public bool? FollowDrafted { get; set; }
        public bool? FollowFieldwork { get; set; }
        public bool? Released { get; set; }
        public string AttackTargetId { get; set; }
        /// <summary>Clear the native attack target instead; mutually exclusive with AttackTargetId.</summary>
        public bool ClearAttackTarget { get; set; }
        public bool DryRun { get; set; }
    }
}
