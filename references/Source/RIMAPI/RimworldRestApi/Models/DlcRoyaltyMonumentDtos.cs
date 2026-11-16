// AI-FORK: Royalty monument marker operation DTOs (RY-R06).
namespace RIMAPI.Models
{
    public sealed class RoyalMonumentRequest
    {
        public int MapId { get; set; }

        /// <summary>ThingID of the spawned MonumentMarker.</summary>
        public string MarkerId { get; set; }

        /// <summary>place_blueprints or cancel.</summary>
        public string Action { get; set; }

        /// <summary>Material for MadeFromStuff sketch things; required when several are allowed.</summary>
        public string StuffDef { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
