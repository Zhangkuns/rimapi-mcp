namespace RIMAPI.Models
{
    public sealed class BookcaseEjectRequest
    {
        public string ThingId { get; set; }
        public string BookId { get; set; }
        public int? MapId { get; set; }
        public bool DryRun { get; set; }
    }
}
