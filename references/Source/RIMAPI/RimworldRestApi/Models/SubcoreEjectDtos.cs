// AI-FORK: BT-12b ripscanner eject/cancel.
namespace RIMAPI.Models
{
    public sealed class SubcoreEjectRequest
    {
        public string ScannerId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
