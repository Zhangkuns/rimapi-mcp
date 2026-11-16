// AI-FORK: BT-12a subcore scanner init/load/enter.
namespace RIMAPI.Models
{
    public sealed class SubcoreInitRequest
    {
        public string ScannerId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class SubcoreLoadRequest
    {
        public string PawnId { get; set; }
        public string ScannerId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class SubcoreEnterRequest
    {
        public string PawnId { get; set; }
        public string ScannerId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
