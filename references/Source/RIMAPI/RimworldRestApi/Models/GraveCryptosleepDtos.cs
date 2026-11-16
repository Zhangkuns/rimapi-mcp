namespace RIMAPI.Models
{
    public sealed class GraveAssignRequest
    {
        public string ThingId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class CryptosleepEjectRequest
    {
        public string ThingId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class CryptosleepCarryRequest
    {
        public string CarrierPawnId { get; set; }
        public string PawnId { get; set; }
        public string CasketId { get; set; }
        public bool DryRun { get; set; }
    }
}
