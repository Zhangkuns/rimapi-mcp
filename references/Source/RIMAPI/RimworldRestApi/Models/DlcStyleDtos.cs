// AI-FORK: DLC ideology style DTOs (G-32).
namespace RIMAPI.Models
{
    public class StyleApplyRequest
    {
        public int ThingId { get; set; }
        public string StyleDef { get; set; }
        public bool Basic { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
