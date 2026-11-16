// AI-FORK: Phase 2a DTOs — map areas, game conditions, apparel, surgery.
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class MapAreaRequest
    {
        public int MapId { get; set; }
        public string Label { get; set; }
        public bool DryRun { get; set; }
    }

    public class AreaCellsRequest
    {
        public int MapId { get; set; }
        public string Area { get; set; }
        public List<PositionDto> Cells { get; set; }
        public PositionDto PointA { get; set; }
        public PositionDto PointB { get; set; }
        public bool Value { get; set; } = true;
        public bool DryRun { get; set; }
    }

    public class PawnAreaRequest
    {
        public string PawnId { get; set; }
        public string Area { get; set; }
        public bool DryRun { get; set; }
    }

    public class GameConditionRequest
    {
        public int MapId { get; set; }
        public string ConditionDef { get; set; }

        /// <summary>Explicit duration; 0 falls back to DurationDays.</summary>
        public int DurationTicks { get; set; }

        public float DurationDays { get; set; }
        public bool DryRun { get; set; }
    }

    public class EndConditionRequest
    {
        public int MapId { get; set; }
        public string ConditionDef { get; set; }
        public bool All { get; set; }
        public bool DryRun { get; set; }
    }

    public class ApparelActionRequest
    {
        public string PawnId { get; set; }
        public int ThingId { get; set; }
        public bool DryRun { get; set; }
    }

    public class ApparelForceRequest
    {
        public string PawnId { get; set; }
        public int ThingId { get; set; }
        public bool Forced { get; set; } = true;
        public bool DryRun { get; set; }
    }

    public class SurgeryRequest
    {
        public string PawnId { get; set; }
        public string RecipeDef { get; set; }
        public string BodyPart { get; set; }

        /// <summary>Exact BodyPartRecord.Index for left/right or duplicate parts.</summary>
        public int? BodyPartId { get; set; }

        /// <summary>
        /// Optional item bound to the surgery (for example the Xenogerm for
        /// ImplantXenogerm); accepts ThingID or numeric thingIDNumber.
        /// </summary>
        public string ThingId { get; set; }

        /// <summary>
        /// Optional doctor to take the native DoBill job (the same job the Doctor
        /// work giver builds); without it the colony AI assigns the surgery.
        /// </summary>
        public string DoctorId { get; set; }

        /// <summary>
        /// Confirms a surgery whose native menu shows a Yes/No dialog
        /// (RecipeWorker.GetConfirmation or the embryo chance warning). The first
        /// request without it is rejected with 409 and the native text.
        /// </summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    public class PregnancyApproachRequest
    {
        public string PawnId { get; set; }
        public string PartnerId { get; set; }
        public string Approach { get; set; }
        public bool DryRun { get; set; }
    }

    public class BabyAutofeedRequest
    {
        public string FeederId { get; set; }
        public string BabyId { get; set; }
        public bool DryRun { get; set; }
    }

    public class BabyCarryRequest
    {
        public string HaulerId { get; set; }
        public string BabyId { get; set; }
        public bool DryRun { get; set; }
    }

    public class BreastfeedRequest
    {
        public string MotherId { get; set; }
        public string BabyId { get; set; }
        public bool DryRun { get; set; }
    }

    public class BabyFeedRequest
    {
        public string FeederId { get; set; }
        public string BabyId { get; set; }
        public string FoodId { get; set; }
        public bool DryRun { get; set; }
    }

    public class GrowthMomentChoiceRequest
    {
        public int? LetterId { get; set; }
        public List<string> PassionDefs { get; set; }
        public string TraitDef { get; set; }
        public bool NoTrait { get; set; }
        public bool DryRun { get; set; }
    }

    public class SurgeryRemoveRequest
    {
        public string PawnId { get; set; }
        public string RecipeDef { get; set; }
        /// <summary>Exact native Bill.loadID; when set, only this bill is removed.</summary>
        public int? BillId { get; set; }
        public bool DryRun { get; set; }
    }

    public class SurgeryBillRequest
    {
        public string PawnId { get; set; }
        public int BillId { get; set; }
        public bool? Suspended { get; set; }
        /// <summary>-1 moves up, 1 moves down.</summary>
        public int? Move { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>
    /// Normal <c>CompUsable</c> use: the same path as the item float menu
    /// (availability report, confirmation text, optional CompTargetable selection,
    /// and the native job with count 1).
    /// </summary>
    public class UseItemRequest
    {
        public string PawnId { get; set; }

        /// <summary>Item ThingID or numeric thingIDNumber (map, inventory or worn).</summary>
        public string ThingId { get; set; }

        /// <summary>
        /// Required for CompTargetable items (for example the ghoul resurrection
        /// serum needs the exact corpse the player would click).
        /// </summary>
        public string TargetThingId { get; set; }

        /// <summary>Confirms every native CompUseEffect.ConfirmMessage dialog.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    public class AreaEditRequest
    {
        public int MapId { get; set; }
        public int? AreaId { get; set; }
        public string Label { get; set; }
        public string Action { get; set; }
        public string NewLabel { get; set; }
        public string Color { get; set; }
        public bool DryRun { get; set; }
    }

    public class AreaPolicyRequest
    {
        public int MapId { get; set; }
        public bool? AutoHomeArea { get; set; }
        public bool? AutoRebuild { get; set; }
        public bool DryRun { get; set; }
    }
}
