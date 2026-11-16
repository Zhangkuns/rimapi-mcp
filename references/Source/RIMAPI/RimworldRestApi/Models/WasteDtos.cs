// AI-FORK: BT-12d wastepack atomizer read/haul.
namespace RIMAPI.Models
{
    public sealed class WasteAtomizeRequest
    {
        public string PawnId { get; set; }
        public string AtomizerId { get; set; }
        public bool DryRun { get; set; }
    }
}
