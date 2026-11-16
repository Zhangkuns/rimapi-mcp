namespace RIMAPI.Models
{
    public class ContainmentTargetRequest
    {
        public int? MapId { get; set; }
        public int? PlatformId { get; set; }
        public string EntityId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class ContainmentModeRequest : ContainmentTargetRequest
    {
        public string Mode { get; set; }
    }

    /// <summary>
    /// AN-R09: the ITab_Entity "extract bioferrite" checkbox for one held entity.
    /// </summary>
    public sealed class ExtractBioferriteRequest : ContainmentTargetRequest
    {
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// AN-R04: the dead pawn's native CommandSelfResurrect gizmo.
    /// </summary>
    public sealed class DeathRefusalRequest
    {
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class ContainmentWorkRequest : ContainmentTargetRequest
    {
        public string PawnId { get; set; }
        public string Action { get; set; }
    }

    public sealed class ContainmentTransferRequest : ContainmentTargetRequest
    {
        public int? DestinationPlatformId { get; set; }
        public string PawnId { get; set; }
    }

    /// <summary>
    /// Cancels the pending capture or transfer order for one exact entity,
    /// like the player drafting the carrier or ending the ordered job. The
    /// entity, platform and containment policy stay unchanged.
    /// </summary>
    public sealed class ContainmentCancelRequest
    {
        public int? MapId { get; set; }
        public string EntityId { get; set; }
        public bool DryRun { get; set; }
    }
}
