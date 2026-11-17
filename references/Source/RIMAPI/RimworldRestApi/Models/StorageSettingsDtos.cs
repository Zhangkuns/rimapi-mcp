using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class StorageSettingsRequest
    {
        public string ThingId { get; set; }
        public int? ZoneId { get; set; }
        public int? MapId { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
        public List<string> AllowedItemDefs { get; set; }
        public List<string> AllowedItemCategories { get; set; }
        public List<string> AddItemDefs { get; set; }
        public List<string> RemoveItemDefs { get; set; }
        public List<string> AddItemCategories { get; set; }
        public List<string> RemoveItemCategories { get; set; }
        public Dictionary<string, bool> SpecialFilters { get; set; }
        public string CopyFromThingId { get; set; }
        public int? CopyFromZoneId { get; set; }
        public int? CopyFromMapId { get; set; }
        public string Priority { get; set; }
        public string FilterDef { get; set; }
        public bool? FilterAllowed { get; set; }
        public string GroupLabel { get; set; }
        public bool? CreateGroup { get; set; }
        public bool? EnrollGroup { get; set; }
        public int? GroupId { get; set; }
        public bool? LeaveGroup { get; set; }
        public string SpecialFilter { get; set; }
        public bool? SpecialFilterAllowed { get; set; }
        public float? MinHitPointsPercent { get; set; }
        public float? MaxHitPointsPercent { get; set; }
        public float? MinMentalBreakChance { get; set; }
        public float? MaxMentalBreakChance { get; set; }
        public string MinQuality { get; set; }
        public string MaxQuality { get; set; }
        public bool DryRun { get; set; }
    }
}
