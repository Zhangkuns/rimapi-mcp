namespace RIMAPI.Models
{
    public sealed class GeneAssembleCancelRequest
    {
        public int? MapId { get; set; }
        public string AssemblerId { get; set; }
        public bool DryRun { get; set; }
    }
}
