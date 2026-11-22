using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class InteractionDefDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
    }

    public class PawnInteractionStatusDto
    {
        public bool CanInteract { get; set; }
        public int CooldownTicks { get; set; }
        public float CooldownDays { get; set; }
        public string LastInteractionDef { get; set; }
        public int LastInteractionTicks { get; set; }
    }

    public class PawnInteractionLogDto
    {
        public int PawnId { get; set; }
        public List<InteractionLogEntryDto> Interactions { get; set; }
        public int Count { get; set; }
    }

    public class InteractionLogEntryDto
    {
        public int InitiatorId { get; set; }
        public string InitiatorName { get; set; }
        public int RecipientId { get; set; }
        public string RecipientName { get; set; }
        public string InteractionDefName { get; set; }
        public string InteractionLabel { get; set; }
        public string Text { get; set; }
        public int Ticks { get; set; }
        public int TickAbs { get; set; }
        public int AgeTicks { get; set; }
        public string TimeAgo { get; set; }
    }

    public class PawnOpinionDto
    {
        public int TargetPawnId { get; set; }
        public string TargetPawnName { get; set; }
        public int Opinion { get; set; }
        public List<OpinionBreakdownDto> Breakdown { get; set; }
    }

    public class OpinionBreakdownDto
    {
        public string ThoughtDefName { get; set; }
        public string Label { get; set; }
        public float Score { get; set; }
    }

    public class ForceInteractionRequestDto
    {
        public int InitiatorId { get; set; }
        public int RecipientId { get; set; }
        public string InteractionDefName { get; set; }
    }

    public class AddRelationRequestDto
    {
        public int Pawn1Id { get; set; }
        public int Pawn2Id { get; set; }
        public string RelationDefName { get; set; }
    }

    public class RemoveRelationRequestDto
    {
        public int Pawn1Id { get; set; }
        public int Pawn2Id { get; set; }
        public string RelationDefName { get; set; }
    }

    public class PawnRelationsDto
    {
        public int PawnId { get; set; }
        public List<PawnRelationEntryDto> Relations { get; set; }
    }

    // ---- PAWN-Q01 merged log ----
    public class PawnLogDto
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public int TicksGame { get; set; }
        public int TicksAbs { get; set; }
        public string TimeBasis { get; set; }
        public string Scope { get; set; }
        public int? SinceTick { get; set; }
        public int? UntilTick { get; set; }
        public int Count { get; set; }
        public int TotalSocial { get; set; }
        public int TotalBattle { get; set; }
        public int NegativeAgeCount { get; set; }
        public List<PawnLogEntryDto> Entries { get; set; }
    }

    public class PawnLogEntryDto
    {
        public int Order { get; set; }
        public string Kind { get; set; } // social, battle
        public string EntryType { get; set; }
        public int LogId { get; set; }
        public int TickAbs { get; set; }
        public int AgeTicks { get; set; }
        public float AgeDays { get; set; }
        public string TimeAgo { get; set; }
        public string Text { get; set; }
        public int InitiatorId { get; set; }
        public string InitiatorName { get; set; }
        public int RecipientId { get; set; }
        public string RecipientName { get; set; }
        public string InteractionDefName { get; set; }
        public string InteractionLabel { get; set; }
        public string BattleId { get; set; }
        public string BattleName { get; set; }
        public int? BattleIndex { get; set; }
        public bool? Hit { get; set; }
        public bool? Deflected { get; set; }
        public List<string> DamagedParts { get; set; }
        public bool ShowInCompactView { get; set; }
    }

    // ---- PAWN-Q03 social / beliefs ----
    public class PawnSocialOverviewDto
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public string Name { get; set; }
        public string Identity { get; set; }
        public string Faction { get; set; }
        public PawnBeliefsDto Beliefs { get; set; }
        public List<PawnSocialEntryDto> Others { get; set; }
        public List<PawnRelationEntryDto> DirectRelations { get; set; }
    }

    public class PawnSocialEntryDto
    {
        public int OtherPawnId { get; set; }
        public string ThingId { get; set; }
        public string Name { get; set; }
        public string Identity { get; set; }
        public string Faction { get; set; }
        public int OpinionOfOther { get; set; }
        public int OpinionOfMe { get; set; }
        public List<PawnRelationEntryDto> Relations { get; set; }
        public string RoleDefName { get; set; }
        public string RoleLabel { get; set; }
    }

    public class PawnBeliefsDto
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public bool HasIdeo { get; set; }
        public int? IdeoId { get; set; }
        public string IdeoName { get; set; }
        public float Certainty { get; set; }
        public float CertaintyPercent { get; set; }
        public float CertaintyChangePerDay { get; set; }
        public float CertaintyChangePercentPerDay { get; set; }
        public bool Prisoner { get; set; }
        public bool Slave { get; set; }
        public string ConversionMode { get; set; }
        public bool ConversionEnabled { get; set; }
        public string RoleDefName { get; set; }
        public string RoleLabel { get; set; }
    }

    public class PawnRelationEntryDto
    {
        public int OtherPawnId { get; set; }
        public string OtherPawnName { get; set; }
        public string RelationDefName { get; set; }
        public string RelationLabel { get; set; }
    }
}
