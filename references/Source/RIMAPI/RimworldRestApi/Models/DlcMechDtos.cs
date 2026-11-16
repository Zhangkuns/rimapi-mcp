// AI-FORK: DLC mech control DTOs (Biotech).
namespace RIMAPI.Models
{
    public class MechBindRequest
    {
        public string PawnId { get; set; }
        public string MechId { get; set; }
        public bool DryRun { get; set; }
    }

    public class MechDisassembleRequest
    {
        public string PawnId { get; set; }
        public string MechId { get; set; }
        public bool DryRun { get; set; }
    }

    public class MechGestatorHaulRequest
    {
        public string GestatorId { get; set; }
        public string ThingId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public class MechGestatorEjectRequest
    {
        public string GestatorId { get; set; }
        public bool DryRun { get; set; }
    }

    public class MechGestatorStartRequest
    {
        public string GestatorId { get; set; }
        public string MechanitorId { get; set; }
        public bool DryRun { get; set; }
    }

    public class MechResurrectRequest
    {
        public string CasterId { get; set; }
        public string CorpseId { get; set; }
        public bool DryRun { get; set; }
    }

    public class MechChargeRequest
    {
        public int? MapId { get; set; }
        public string MechId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public class MechModeRequest
    {
        public string PawnId { get; set; }

        /// <summary>Exact group_index from GET; the endpoint never guesses it.</summary>
        public int? GroupIndex { get; set; }

        public string ModeDef { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>BT-11f: move one exact mech into an existing control group.</summary>
    public class MechGroupAssignRequest
    {
        public string PawnId { get; set; }
        public string MechId { get; set; }
        public int? GroupIndex { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>
    /// BT-R18: set the native CompMechCarrier autofill target (the gizmo slider
    /// value in fixedIngredient units, bounded by max_ingredient_count).
    /// </summary>
    public class MechCarrierAutofillRequest
    {
        public string MechId { get; set; }
        public int? MaxToFill { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>BT-R18: native CompMechCarrier.TrySpawnPawns release.</summary>
    public class MechCarrierReleaseRequest
    {
        public string MechId { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>BT-11f: write the native per-group recharge thresholds.</summary>
    public class MechGroupRechargeRequest
    {
        public string PawnId { get; set; }
        public int? GroupIndex { get; set; }
        public float? RechargeMin { get; set; }
        public float? RechargeMax { get; set; }
        public bool DryRun { get; set; }
    }
}
