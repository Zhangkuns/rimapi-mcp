namespace RIMAPI.Models
{
    /// <summary>BT-R16: native CompBandNode.TuneTo; no manual tick field write.</summary>
    public sealed class BandNodeTuneRequest
    {
        public int? MapId { get; set; }
        public string NodeId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }
}
