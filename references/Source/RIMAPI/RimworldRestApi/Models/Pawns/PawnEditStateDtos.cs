using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>WORK-P01: set one prisoner's native Recruitable flag.</summary>
    public class PawnRecruitableRequest
    {
        public int PawnId { get; set; }
        /// <summary>Explicit target value; omitted is rejected.</summary>
        public bool? Recruitable { get; set; }
    }

    /// <summary>WORK-P01: remove one exact Trait(def, degree) from a pawn.</summary>
    public class PawnTraitRemoveRequest
    {
        public int PawnId { get; set; }
        public string DefName { get; set; }
        /// <summary>Required when the def has several degrees.</summary>
        public int? Degree { get; set; }
        /// <summary>Allow removing a gene-derived trait, which also removes its source gene.</summary>
        public bool AllowGeneSource { get; set; }
    }

    /// <summary>WORK-P01: remove one selected mood or social memory.</summary>
    public class PawnMemoryRemoveRequest
    {
        public int PawnId { get; set; }
        /// <summary>mood or social; situational/environment thoughts are not memories.</summary>
        public string Kind { get; set; }
        public string DefName { get; set; }
        /// <summary>Zero-based index among the current matching memories.</summary>
        public int? Index { get; set; }
    }

    /// <summary>WORK-P01: end the pawn's current inspiration.</summary>
    public class PawnInspirationEndRequest
    {
        public int PawnId { get; set; }
    }

    /// <summary>WORK-P01: replace a childhood or adulthood backstory.</summary>
    public class PawnBackstoryRequest
    {
        public int PawnId { get; set; }
        /// <summary>childhood or adulthood.</summary>
        public string Slot { get; set; }
        public string DefName { get; set; }
    }

    /// <summary>WORK-W04: set in-level experience without crossing a skill level.</summary>
    public class PawnSkillXpRequest
    {
        public int PawnId { get; set; }
        public List<PawnSkillXpEntry> Skills { get; set; }
    }

    public class PawnSkillXpEntry
    {
        public string SkillName { get; set; }
        public float? XpSinceLastLevel { get; set; }
    }

    /// <summary>WORK-W03: add one mood or social memory from an explicit ThoughtDef.</summary>
    public class PawnMemoryAddRequest
    {
        public int PawnId { get; set; }
        public string DefName { get; set; }
        /// <summary>mood or social; omitted infers from the ThoughtDef class.</summary>
        public string Kind { get; set; }
        public int? OtherPawnId { get; set; }
    }

    /// <summary>WORK-W03: set a colony prisoner's ideoligion directly.</summary>
    public class PawnSetIdeoRequest
    {
        public string PawnId { get; set; }
        public string IdeoId { get; set; }
        public bool DryRun { get; set; }
    }

    /// <summary>WORK-W02: edit one exact carried item instance.</summary>
    public class PawnItemAttributesRequest
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public int? HitPoints { get; set; }
        /// <summary>QualityCategory name (Awful..Legendary) or its integer.</summary>
        public string Quality { get; set; }
    }

    /// <summary>WORK-P01: set a prisoner's resistance and/or will directly.</summary>
    public class PawnPrisonerStateRequest
    {
        public int PawnId { get; set; }
        public float? Resistance { get; set; }
        public float? Will { get; set; }
    }
}
