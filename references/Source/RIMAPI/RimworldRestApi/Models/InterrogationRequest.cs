namespace RIMAPI.Models
{
    public sealed class InterrogationRequest
    {
        public int? MapId { get; set; }
        public string WardenId { get; set; }
        public string PrisonerId { get; set; }
        public bool DryRun { get; set; }
    }
}
