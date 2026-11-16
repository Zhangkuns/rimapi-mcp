// AI-FORK: DLC ideology DTOs (G-30, G-31).
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class IdeoConvertRequest
    {
        public string PawnId { get; set; }

        /// <summary>Native Ideo.id as string; ids are per-save and not stable across worlds.</summary>
        public string IdeoId { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public class RitualCancelRequest
    {
        public string RunId { get; set; }
        public bool DryRun { get; set; }
    }

    // G-48: relic precept read and native install/extract jobs.
    public sealed class RelicInstallRequest
    {
        public int? MapId { get; set; }
        public string RelicId { get; set; }
        public string ReliquaryId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class RelicExtractRequest
    {
        public int? MapId { get; set; }
        public string ReliquaryId { get; set; }
        public string PawnId { get; set; }

        /// <summary>Optional explicit storage cell; omitted uses the native storage search.</summary>
        public int? StorageX { get; set; }

        public int? StorageZ { get; set; }
        public bool DryRun { get; set; }
    }

    // G-47: normal Convert ability (CompAbilityEffect_Convert) on an exact target.
    public sealed class IdeoConvertAbilityRequest
    {
        public string CasterId { get; set; }
        public string TargetId { get; set; }
        public bool DryRun { get; set; }
    }

    // G-80: set the native prisoner interaction mode to Convert and the target ideo.
    public sealed class IdeoStyleItemEdit
    {
        public string DefName { get; set; }
        /// <summary>Never/Rare/Uncommon/Normal/Common/Frequent.</summary>
        public string Frequency { get; set; }
        /// <summary>Male/MaleUsually/Any/FemaleUsually/Female.</summary>
        public string Gender { get; set; }
    }

    public sealed class IdeoReformStyleRequest
    {
        public string IdeoId { get; set; }
        public string StyleDef { get; set; }
        /// <summary>add or remove.</summary>
        public string Action { get; set; }

        /// <summary>
        /// Combined appearance edit: hair/beard/tattoo frequency and gender changes
        /// applied together with StyleCategories in one normal reform.
        /// </summary>
        public List<IdeoStyleItemEdit> StyleItems { get; set; }

        /// <summary>Full ordered building style slot list (0..3) for the same reform.</summary>
        public List<string> StyleCategories { get; set; }

        /// <summary>
        /// Classic-mode Dialog_StyleSelection list (0..3) written directly to
        /// IdeoManager.selectedStyleCategories without spending reform points.
        /// </summary>
        public List<string> ClassicStyleCategories { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoReformPreceptRequest
    {
        public string IdeoId { get; set; }
        public int? PreceptId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>
    /// ID-06a: assign or clear one native Ideology role precept. The role must be
    /// an existing Precept_Role in the player's primary ideo; a null pawn_id clears it.
    /// </summary>
    public sealed class IdeoRoleAssignRequest
    {
        public int? PreceptId { get; set; }
        public string PawnId { get; set; }

        /// <summary>
        /// ID-R16 optional player-faction ideo id (primary or minor). Omitted keeps
        /// the primary ideo. Leader roles stay primary-only, as in SocialCardUtility.
        /// </summary>
        public string IdeoId { get; set; }

        public bool DryRun { get; set; }
    }

    /// <summary>
    /// ID-07a: change the two policy values the native pruning gizmo and dryad
    /// caste dialog write on a connected Gauranlen tree. Pruning and the caste
    /// swap themselves stay with the native WorkGivers.
    /// </summary>
    /// <summary>Native "heal in healing pod" toggle on one connected dryad.</summary>
    public sealed class IdeoTreeDryadRequest
    {
        /// <summary>Numeric thing ID of the Dryad_* pawn.</summary>
        public int? ThingId { get; set; }

        /// <summary>New mindState.returnToHealingPod value.</summary>
        public bool? ReturnToHealingPod { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class IdeoTreePolicyRequest
    {
        /// <summary>Numeric thing ID of the Plant_TreeGauranlen.</summary>
        public int? ThingId { get; set; }

        /// <summary>Desired connection strength, 0..1 (native setter also clamps).</summary>
        public float? DesiredStrength { get; set; }

        /// <summary>GauranlenTreeModeDef defName for the desired dryad caste.</summary>
        public string DesiredMode { get; set; }

        public bool DryRun { get; set; }
    }

    /// <summary>
    /// ID-08c: apply one native styling look change. Exactly one of the hair,
    /// beard, tattoo or hair_color fields is required; the styling station and
    /// the native UseStylingStation/DyeHair jobs perform the change.
    /// </summary>
    /// <summary>
    /// ID-06b: the player-editable fields of one Precept_Ritual instance:
    /// name, the native Anytime/date starting-condition toggle, the attachable
    /// outcome reward and the date trigger. Omitted fields stay unchanged.
    /// </summary>
    /// <summary>ID-08d: order the native neural supercharge job on one building.</summary>
    /// <summary>ID-08b: set the prisoner's Enslave mode and order a warden.</summary>
    /// <summary>OD-07b: set an animal training checkbox and optionally order a native Train job.</summary>
    public sealed class AnimalTrainingRequest
    {
        public string AnimalId { get; set; }
        public string Trainable { get; set; }
        public bool Wanted { get; set; } = true;
        public bool TrainNow { get; set; }
        public string TrainerId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoSlaveryEnslaveRequest
    {
        public int? MapId { get; set; }
        public string PawnId { get; set; }
        public string WardenId { get; set; }
        /// <summary>Run the native warden job instead of converting immediately.</summary>
        public bool UseWardenJob { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class NeuralSuperchargerUseRequest
    {
        public int? MapId { get; set; }
        public string ThingId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoRitualConfigRequest
    {
        public int? RitualId { get; set; }
        public string Name { get; set; }
        public bool? IsAnytime { get; set; }

        /// <summary>RitualAttachableOutcomeEffectDef defName; empty string clears it.</summary>
        public string AttachableOutcomeEffect { get; set; }

        /// <summary>0..59 days since the start of the year; only date-triggered rituals.</summary>
        public int? TriggerDaysSinceStartOfYear { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class IdeoStylingRequest
    {
        public string PawnId { get; set; }

        /// <summary>Optional numeric station id; omitted picks the nearest reachable station.</summary>
        public int? StationId { get; set; }

        public string HairDef { get; set; }
        public string BeardDef { get; set; }
        public string FaceTattooDef { get; set; }
        public string BodyTattooDef { get; set; }

        /// <summary>#RRGGBB hair color; the native DyeHair job consumes one Dye.</summary>
        public string HairColor { get; set; }

        /// <summary>Worn apparel ThingID for the native RecolorApparel job.</summary>
        public string ApparelId { get; set; }

        /// <summary>#RRGGBB target color for the native RecolorApparel job.</summary>
        public string ApparelColor { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class IdeoReformPreceptAddRequest
    {
        public string IdeoId { get; set; }
        public string PreceptDef { get; set; }

        /// <summary>
        /// Optional ThingDef for building/relic precepts. When omitted the native
        /// Precept_ThingDef.Init picks a legal def from def.Worker.ThingDefsForIdeo.
        /// </summary>
        public string ThingDef { get; set; }

        /// <summary>
        /// Apparel ThingDef for Precept_Apparel. Must be in the def's native
        /// Worker.ThingDefsForIdeo set and not already desired by this ideo.
        /// </summary>
        public string ApparelDef { get; set; }

        /// <summary>
        /// Noble/despised WeaponClassDef names for Precept_Weapon. The pair must match
        /// one native WeaponClassPairDef (first=noble, second=despised).
        /// </summary>
        public string NobleWeaponClass { get; set; }

        public string DespisedWeaponClass { get; set; }

        /// <summary>XenotypeDef name for Precept_Xenotype (preferred xenotype).</summary>
        public string XenotypeDef { get; set; }

        /// <summary>
        /// Optional RitualPatternDef for a ritual precept. Omitted uses
        /// def.ritualPatternBase. The value must be a native-legal group or meme
        /// replacement pattern from GET precepts?include_add_options=true.
        /// </summary>
        public string RitualPattern { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoPreceptEditRequest
    {
        public string IdeoId { get; set; }
        public int? PreceptId { get; set; }

        /// <summary>Precept label. For a leader-role precept use LeaderTitleMale.</summary>
        public string Name { get; set; }

        /// <summary>Native precept name lock; not available on leader-role precepts.</summary>
        public bool? NameLocked { get; set; }

        /// <summary>Leader role titles; the male title is also the precept name.</summary>
        public string LeaderTitleMale { get; set; }

        public string LeaderTitleFemale { get; set; }

        /// <summary>
        /// Index into edit-options addable_apparel_requirements. Adding the entry uses
        /// the native Precept_Role.AllPossibleRequirements(desperate:true) list.
        /// </summary>
        public int? AddApparelRequirementIndex { get; set; }

        /// <summary>Index into the current apparel_requirements list.</summary>
        public int? RemoveApparelRequirementIndex { get; set; }

        /// <summary>ThingDef stuff for a relic whose ThingDef is MadeFromStuff.</summary>
        public string RelicStuff { get; set; }

        /// <summary>ThingStyleDef defName for a building precept.</summary>
        public string BuildingStyle { get; set; }

        /// <summary>Optional StyleCategoryDef to disambiguate a duplicate style defName.</summary>
        public string BuildingStyleCategory { get; set; }

        /// <summary>ThingDef for a Precept_RitualSeat replacement.</summary>
        public string RitualSeatDef { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoReformPreceptReplaceRequest
    {
        public string IdeoId { get; set; }
        public int? PreceptId { get; set; }
        public string ReplacementDef { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoReformMemeRequest
    {
        public string IdeoId { get; set; }
        public string MemeDef { get; set; }
        /// <summary>add or remove.</summary>
        public string Action { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoReformStructureMemeRequest
    {
        public string IdeoId { get; set; }
        public string MemeDef { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    // ID-R15: one normal fluid reform draft, applied once through the native
    // Dialog_ReformIdeo path (IdeoDevelopmentUtility.ApplyChangesToIdeo). The
    // native dialog allows at most one "choose one" change — structure meme,
    // normal memes or building style slots — plus any number of narrative,
    // precept, appearance and identity edits. The combined section therefore
    // mirrors that rule; culture/foundation are never opened.
    public sealed class IdeoReformCombinedRequest
    {
        public string IdeoId { get; set; }

        /// <summary>Choose-one: replacement structure meme.</summary>
        public string StructureMeme { get; set; }

        /// <summary>Choose-one: normal memes to add.</summary>
        public List<string> MemesAdd { get; set; }

        /// <summary>Choose-one: normal memes to remove.</summary>
        public List<string> MemesRemove { get; set; }

        /// <summary>Appearance items (hair/beard/tattoo) in the change-any group.</summary>
        public List<IdeoStyleItemEdit> StyleItems { get; set; }

        /// <summary>
        /// Choose-one: full ordered building style slot list (0..3). Null keeps
        /// the slots; an empty list clears them.
        /// </summary>
        public List<string> StyleCategories { get; set; }

        /// <summary>Plain PreceptDefs added to the draft (change-any).</summary>
        public List<string> PreceptsAdd { get; set; }

        /// <summary>Existing precept ids removed from the draft (change-any).</summary>
        public List<int> PreceptsRemove { get; set; }

        public string Name { get; set; }
        public string Adjective { get; set; }
        public string MemberName { get; set; }
        public string IconDef { get; set; }
        public string ColorDef { get; set; }

        /// <summary>
        /// Custom narrative text. Setting it locks the description
        /// (descriptionLocked=true) unless description_locked overrides it.
        /// </summary>
        public string Description { get; set; }

        /// <summary>Explicit narrative lock toggle; overrides the implicit lock.</summary>
        public bool? DescriptionLocked { get; set; }

        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoReformIdentityRequest
    {
        public string IdeoId { get; set; }
        public string Name { get; set; }
        public string Adjective { get; set; }
        public string MemberName { get; set; }
        public string Description { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoReformSymbolRequest
    {
        public string IdeoId { get; set; }
        public string IconDef { get; set; }
        public string ColorDef { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoReformDeityRequest
    {
        public string IdeoId { get; set; }
        /// <summary>edit, remove or randomize_all.</summary>
        public string Action { get; set; }
        public int? DeityIndex { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Gender { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class PrisonerConvertRequest
    {
        public string PawnId { get; set; }

        /// <summary>Native Ideo.id as string; omitted uses the player primary ideo.</summary>
        public string IdeoId { get; set; }

        public bool DryRun { get; set; }
    }

    // G-46: biosculpter pod normal enter/eject flow.
    public sealed class BiosculpterEnterRequest
    {
        public int? MapId { get; set; }

        /// <summary>ThingID of the player biosculpter pod.</summary>
        public string PodId { get; set; }

        public string PawnId { get; set; }

        /// <summary>
        /// Optional other pawn that carries a downed colonist or a prisoner into the
        /// pod (native CarryToBiosculpterPod). Omit it to order the pawn itself in.
        /// </summary>
        public string CarrierPawnId { get; set; }

        /// <summary>Native cycle key, for example "pleasure", "ageReversal", "medic".</summary>
        public string CycleKey { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class BiosculpterPolicyRequest
    {
        public int? MapId { get; set; }
        public string PodId { get; set; }
        public bool? AutoLoadNutrition { get; set; }
        public bool? AutoAgeReversal { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class IdeoPresetSaveRequest
    {
        /// <summary>Optional player-faction ideo id; omitted uses the primary ideo.</summary>
        public string IdeoId { get; set; }

        /// <summary>Preset name, written as &lt;name&gt;.rid through the native saver.</summary>
        public string FileName { get; set; }

        /// <summary>Replace an existing preset file; default false keeps it intact.</summary>
        public bool Overwrite { get; set; }

        public bool DryRun { get; set; }
    }

    public sealed class BiosculpterEjectRequest
    {
        public int? MapId { get; set; }
        public string PodId { get; set; }

        /// <summary>Default true: interrupted ejection adds BiosculptingSickness.</summary>
        public bool? Interrupted { get; set; }

        public bool DryRun { get; set; }
    }

    // ID-R14: one explicit role/pawn row for a ritual roster. Repeat a role id
    // only for roles whose native RitualRole.MaxCount allows more than one pawn.
    public sealed class RitualRoleAssignmentRequest
    {
        public string RoleId { get; set; }
        public string PawnId { get; set; }
    }

    // G-31: start one exact Precept_Ritual instance through its native behavior.
    // `RitualId` is the native Precept.Id (see GET /dlc/ideology/ritual), not a Def
    // name: one Def can back several ritual instances with different names.
    public class RitualStartRequest
    {
        public int RitualId { get; set; }

        /// <summary>
        /// ID-R16 optional player-faction ideo id (primary or minor). When given,
        /// ritual_id must belong to that ideo; omitted searches every player ideo.
        /// </summary>
        public string IdeoId { get; set; }

        /// <summary>Map unique id; omitted means Find.CurrentMap.</summary>
        public int? MapId { get; set; }

        /// <summary>Target ThingID (for example "RitualSpot12345").</summary>
        public string ThingId { get; set; }

        /// <summary>Target cell, used when ThingId is omitted.</summary>
        public int? TargetX { get; set; }

        public int? TargetZ { get; set; }

        /// <summary>Optional organizer PawnID; when omitted the native dialog's null organizer is used.</summary>
        public string OrganizerPawnId { get; set; }

        /// <summary>Optional forced roles: role id to PawnID.</summary>
        public Dictionary<string, string> Roles { get; set; }

        /// <summary>
        /// ID-R14 optional multi-pawn role roster applied after the native
        /// FillPawns through RitualRoleAssignments.TryAssign. Explicit rows
        /// replace the native auto-assignment for that role. A role already
        /// present in `roles` stays a single forced lock and cannot also appear
        /// here. Each pawn may appear once.
        /// </summary>
        public List<RitualRoleAssignmentRequest> RoleAssignments { get; set; }

        /// <summary>
        /// ID-R14 optional exact spectator roster. When present, every native
        /// spectator candidate not listed is removed from participation with
        /// RemoveParticipant and every listed pawn is assigned with the native
        /// TryAssignSpectate. Omitted keeps the native FillPawns spectator fill.
        /// </summary>
        public List<string> Spectators { get; set; }

        /// <summary>
        /// ID-R14 optional non-participants. Each pawn is removed through the
        /// native RemoveParticipant after the roster is applied. The organizer,
        /// forced role pawns and extra required pawns cannot be excluded.
        /// </summary>
        public List<string> Exclude { get; set; }

        /// <summary>
        /// Optional active RitualObligation.ID. Omitted keeps the legacy
        /// no-obligation/unique-native-match behavior; an explicit id must exist
        /// in this ritual and accept the submitted target, otherwise the request
        /// is rejected instead of silently switching obligations.
        /// </summary>
        public int? ObligationId { get; set; }

        /// <summary>
        /// RoleChange only: explicit target Precept_Role.Id. Omitted keeps the
        /// native default (first eligible role, or removal when the pawn already
        /// holds one); 0 explicitly removes; a positive id selects that role.
        /// </summary>
        public int? RoleChangePreceptId { get; set; }

        public bool DryRun { get; set; }
    }

    // ID-R14: reproduce the native "Leave ritual" gizmo for one owned pawn of
    // one exact recorded run. The organizer, required role pawns, extra required
    // pawns and the last remaining participant cannot leave.
    public sealed class RitualLeaveRequest
    {
        public string RunId { get; set; }
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }
}
