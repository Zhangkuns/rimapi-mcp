namespace RIMAPI.Models
{
    /// <summary>RY-R07: native Faction reward preference flags.</summary>
    public sealed class RewardPrefsRequest
    {
        public int FactionId { get; set; }
        public bool? AllowRoyalFavorRewards { get; set; }
        public bool? AllowGoodwillRewards { get; set; }
        public bool DryRun { get; set; }
    }
}
