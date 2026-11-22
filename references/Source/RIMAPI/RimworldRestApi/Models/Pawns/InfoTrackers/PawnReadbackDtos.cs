using System.Collections.Generic;

namespace RIMAPI.Models
{
    // ---- PAWN-Q02 gear ----
    public class PawnGearDto
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public int Tick { get; set; }
        public float MassCarried { get; set; }
        public float MassCapacity { get; set; }
        public float EncumbrancePercent { get; set; }
        public bool OverEncumbered { get; set; }
        public float ComfyTemperatureMin { get; set; }
        public float ComfyTemperatureMax { get; set; }
        public float ArmorSharp { get; set; }
        public float ArmorBlunt { get; set; }
        public float ArmorHeat { get; set; }
        public List<PawnGearItemDto> Items { get; set; }
    }

    public class PawnGearItemDto
    {
        public string ThingId { get; set; }
        public string DefName { get; set; }
        public string Label { get; set; }
        public string Category { get; set; } // inventory, apparel, equipment
        public int StackCount { get; set; }
        public float Mass { get; set; }
        public float HitPoints { get; set; }
        public float MaxHitPoints { get; set; }
        public string Quality { get; set; }
        public bool Worn { get; set; }
        public bool Equipped { get; set; }
        public float ArmorSharp { get; set; }
        public float ArmorBlunt { get; set; }
        public float ArmorHeat { get; set; }
    }

    // ---- PAWN-Q04 bio ----
    public class PawnBioDto
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public string Name { get; set; }
        public int BiologicalAge { get; set; }
        public float BiologicalAgeFloat { get; set; }
        public long ChronologicalAgeTicks { get; set; }
        public int ChronologicalAge { get; set; }
        public PawnBackstoryReadDto Childhood { get; set; }
        public PawnBackstoryReadDto Adulthood { get; set; }
        public List<PawnTraitReadDto> Traits { get; set; }
        public List<PawnWorkRestrictionDto> WorkRestrictions { get; set; }
        public List<string> DisabledWorkTypes { get; set; }
    }

    public class PawnBackstoryReadDto
    {
        public string DefName { get; set; }
        public string Title { get; set; }
        public string Slot { get; set; }
        public List<string> DisabledWorkTypes { get; set; }
    }

    public class PawnTraitReadDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public int Degree { get; set; }
        public bool Suppressed { get; set; }
        public string Source { get; set; } // natural, gene, scenario
        public string SourceGene { get; set; }
        public int DisabledWorkTags { get; set; }
        public List<string> DisabledWorkTypes { get; set; }
    }

    public class PawnWorkRestrictionDto
    {
        public string WorkTypeDefName { get; set; }
        public string Label { get; set; }
        public string Source { get; set; } // backstory_childhood, backstory_adulthood, trait, gene, ideology_role
        public string SourceDefName { get; set; }
        public string SourceLabel { get; set; }
    }

    // ---- PAWN-Q05 needs ----
    public class PawnNeedsDto
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public int Tick { get; set; }
        public bool HasNeeds { get; set; }
        public string RequestedNeedDef { get; set; }
        public List<PawnNeedReadDto> Needs { get; set; }
        public PawnFoodNeedDto Food { get; set; }
    }

    public class PawnNeedReadDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public float CurLevel { get; set; }
        public float MaxLevel { get; set; }
        public float LevelPercent { get; set; }
        public float CurInstantLevel { get; set; }
        public float CurInstantLevelPercent { get; set; }
        public bool ShowOnNeedList { get; set; }
        public int ChangeArrow { get; set; }
        public string Trend { get; set; } // rising, falling, stable
        public bool Major { get; set; }
        public int ListPriority { get; set; }
        public float FallPerDay { get; set; }
        public string Tip { get; set; }
    }

    public class PawnFoodNeedDto
    {
        public string DefName { get; set; }
        public float CurLevel { get; set; }
        public float MaxLevel { get; set; }
        public float LevelPercent { get; set; }
        public string Category { get; set; }
        public bool Starving { get; set; }
        public float FoodFallPerTick { get; set; }
        public int TicksUntilHungryWhenFed { get; set; }
        public float NutritionWanted { get; set; }
        public float PercentageThreshHungry { get; set; }
        public float PercentageThreshUrgentlyHungry { get; set; }
    }

    // ---- PAWN-Q06 health ----
    public class PawnHealthDto
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public int Tick { get; set; }
        public bool Dead { get; set; }
        public bool Downed { get; set; }
        public float SummaryHealth { get; set; }
        public float PainTotal { get; set; }
        public float PainPercent { get; set; }
        public float BleedRateTotal { get; set; }
        public bool Bleeding { get; set; }
        public int? TicksUntilDeathBloodLoss { get; set; }
        public string BloodLossTime { get; set; }
        public List<PawnCapacityReadDto> Capacities { get; set; }
    }

    public class PawnCapacityReadDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public float Level { get; set; }
        public float Percent { get; set; }
        public bool Capable { get; set; }
        public float MinForCapable { get; set; }
    }

    public class PawnSurgeryOptionsDto
    {
        public int PawnId { get; set; }
        public string ThingId { get; set; }
        public string RecipeDef { get; set; }
        public bool AllRecipes { get; set; }
        public List<PawnSurgeryRecipeDto> Recipes { get; set; }
    }

    public class PawnSurgeryRecipeDto
    {
        public string DefName { get; set; }
        public string Label { get; set; }
        public bool TargetsBodyPart { get; set; }
        public bool Available { get; set; }
        public List<PawnSurgeryPartDto> Parts { get; set; }
    }

    public class PawnSurgeryPartDto
    {
        public int PartId { get; set; }
        public string DefName { get; set; }
        public string Label { get; set; }
        public string CustomLabel { get; set; }
        public float Health { get; set; }
        public bool Missing { get; set; }
    }
}
