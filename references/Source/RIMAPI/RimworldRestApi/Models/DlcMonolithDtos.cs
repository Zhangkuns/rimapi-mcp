// AI-FORK: DLC anomaly monolith DTOs (G-36 / G-43 endgame).
namespace RIMAPI.Models
{
    public class MonolithActivateRequest
    {
        /// <summary>Pawn that performs the native activation.</summary>
        public string PawnId { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>
    /// Orders the native InteractThing work on a void structure or on the
    /// gleaming monolith. The pawn performs the activation over the native
    /// ticksToActivate, exactly like the right-click "activate" order.
    /// </summary>
    public sealed class VoidStructureActivateRequest
    {
        /// <summary>Native thingIDNumber of the void structure (or gleaming monolith).</summary>
        public int? ThingId { get; set; }

        public string PawnId { get; set; }
        public int? MapId { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>
    /// AN-R01: stop the active project of one native anomaly knowledge category.
    /// </summary>
    public sealed class AnomalyKnowledgeStopRequest
    {
        /// <summary>KnowledgeCategoryDef.defName (Basic, Advanced, ...).</summary>
        public string Category { get; set; }

        public bool DryRun { get; set; }
    }

    /// <summary>
    /// AN-R02: the normal per-thing "study" toggle (CompStudiable.SetStudyEnabled).
    /// </summary>
    public sealed class AnomalyStudyRequest
    {
        public int? MapId { get; set; }

        /// <summary>Native thingIDNumber (spawned thing or platform-held pawn).</summary>
        public int? ThingId { get; set; }

        public bool Enabled { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>
    /// Performs the void-node endgame choice. The pawn must be inside the
    /// metal hell pocket map that the gleaming monolith opens.
    /// </summary>
    public sealed class MonolithEndgameRequest
    {
        /// <summary>"disrupt" (level 6, collapsed monolith) or "embrace" (level 5).</summary>
        public string Choice { get; set; }

        public string PawnId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
