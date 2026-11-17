namespace RIMAPI.Models
{
    public sealed class PolicyRenameRequest
    {
        public int? PolicyId { get; set; }
        public int? OutfitId { get; set; }
        public string Label { get; set; }
        public bool DryRun { get; set; }
    }
}
