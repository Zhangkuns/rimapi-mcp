namespace RIMAPI.Models
{
    /// <summary>
    /// OD-R01: the three native CerebrexCore dialog choices. The reference is the
    /// exact building that the player interacts with; destroy/scavenge schedule
    /// the native 300-tick DeactivateCore path, postpone is a no-op.
    /// </summary>
    public sealed class CerebrexChoiceRequest
    {
        public int? MapId { get; set; }
        public int? ThingId { get; set; }
        /// <summary>destroy | scavenge | postpone</summary>
        public string Choice { get; set; }
        public bool DryRun { get; set; }
    }
}
