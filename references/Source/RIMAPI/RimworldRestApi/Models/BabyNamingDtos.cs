namespace RIMAPI.Models
{
    public sealed class BabyNameRequest
    {
        public string PawnId { get; set; }
        public string FirstName { get; set; }
        public string NickName { get; set; }
        public string LastName { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class BabyNameDelayRequest
    {
        public string PawnId { get; set; }
        public int Days { get; set; }
        public bool DryRun { get; set; }
    }
}
