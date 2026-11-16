// AI-FORK: DLC anomaly mutation DTOs (G-39).
namespace RIMAPI.Models
{
    public class MutantTransformRequest
    {
        public string PawnId { get; set; }
        public string MutantDef { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public class MutantRevertRequest
    {
        public string PawnId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>
    /// Native corpse-to-shambler resurrection (MutantUtility.ResurrectAsShambler).
    /// The shambler rises over the native 5-10 seconds; the response reports the
    /// submitted native call and the immediate readback, not the finished rise.
    /// </summary>
    public sealed class MutantResurrectRequest
    {
        /// <summary>ThingID of a spawned corpse on map_id.</summary>
        public string CorpseId { get; set; }

        /// <summary>Optional native lifespan ticks; omitted keeps the native default.</summary>
        public int? LifespanTicks { get; set; }

        public int? MapId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
