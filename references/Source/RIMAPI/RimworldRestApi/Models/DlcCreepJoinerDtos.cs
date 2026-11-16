// AI-FORK: Anomaly creep joiner response (G-87).
namespace RIMAPI.Models
{
    public sealed class CreepJoinerRespondRequest
    {
        public string PawnId { get; set; }

        /// <summary>accept, reject or capture.</summary>
        public string Action { get; set; }

        /// <summary>Required for a non-dry run: accept/reject/capture changes the joiner.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
