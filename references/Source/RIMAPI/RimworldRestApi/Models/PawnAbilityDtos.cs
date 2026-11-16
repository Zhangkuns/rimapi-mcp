using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>
    /// Generic player ability use (AN-07a / ID-08a / BT-13a / RY-05): the same
    /// Ability.GetJob path the native ability gizmo uses, for any ability the pawn
    /// actually holds. Target is one pawn/thing/cell, or omitted for self-cast.
    /// </summary>
    public sealed class AbilityRequest
    {
        public string PawnId { get; set; }
        public string AbilityDef { get; set; }

        public string TargetPawnId { get; set; }
        public string TargetThingId { get; set; }
        public PositionDto Cell { get; set; }

        /// <summary>Destination for two-stage abilities (CompAbilityEffect_WithDest).</summary>
        public PositionDto DestCell { get; set; }

        /// <summary>Pawn destination for two-stage abilities such as WordOfLove.</summary>
        public string DestPawnId { get; set; }

        /// <summary>Target map id for world-target abilities such as Farskip.</summary>
        public int? WorldMapId { get; set; }

        /// <summary>Target caravan id for world-target abilities such as Farskip.</summary>
        public int? WorldCaravanId { get; set; }

        /// <summary>Confirms native ability ConfirmationDialog text when present.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
