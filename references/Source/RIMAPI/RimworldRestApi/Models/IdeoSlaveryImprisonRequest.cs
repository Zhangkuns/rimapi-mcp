namespace RIMAPI.Models
{
    public sealed class IdeoSlaveryImprisonRequest
    {
        public int? MapId { get; set; }
        public string SlaveId { get; set; }
        public string WardenId { get; set; }
        public bool DryRun { get; set; }
    }
}
