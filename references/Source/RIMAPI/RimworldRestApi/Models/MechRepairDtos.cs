// AI-FORK: BT-11g mech repair (native RepairMech job).
namespace RIMAPI.Models
{
    public sealed class MechRepairRequest
    {
        public string PawnId { get; set; }
        public string MechId { get; set; }
        public bool DryRun { get; set; }
    }
}
