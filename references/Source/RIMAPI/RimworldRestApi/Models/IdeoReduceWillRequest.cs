namespace RIMAPI.Models
{
    public sealed class IdeoReduceWillRequest
    {
        public int? MapId { get; set; }
        public string PrisonerId { get; set; }
        public string WardenId { get; set; }
        public bool DryRun { get; set; }
    }
}
