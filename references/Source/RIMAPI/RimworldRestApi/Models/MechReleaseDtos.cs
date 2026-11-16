// AI-FORK: BT-11g mech release (native overseer disconnect).
namespace RIMAPI.Models
{
    public sealed class MechReleaseRequest
    {
        public string PawnId { get; set; }
        public string MechId { get; set; }
        public bool DryRun { get; set; }
    }
}
