// AI-FORK: DLC anomaly containment DTOs (G-37).
namespace RIMAPI.Models
{
    public class ContainmentReleaseRequest
    {
        /// <summary>Native thingIDNumber of the holding platform.</summary>
        public int PlatformId { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
