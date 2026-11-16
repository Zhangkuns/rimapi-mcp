namespace RIMAPI.Models
{
    public sealed class TakeToBedRequest
    {
        public string PatientId { get; set; }
        public string BedId { get; set; }
        public string ActorId { get; set; }
        public string Mode { get; set; }
        public bool DryRun { get; set; }
    }
}
