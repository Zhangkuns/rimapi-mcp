namespace RIMAPI.Models
{
    /// <summary>
    /// OD-R04: read one native CompHackable and optionally evaluate the native
    /// WorkGiver_Hack gate for a pawn.
    /// </summary>
    public sealed class HackableAutoRequest
    {
        public int? MapId { get; set; }
        public string ThingId { get; set; }
        public bool Enabled { get; set; }
        public bool DryRun { get; set; }
    }
}
