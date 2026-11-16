namespace RIMAPI.Models
{
    public sealed class PenPolicyRequest
    {
        public string ThingId { get; set; }
        public string Label { get; set; }
        public bool? AutoCut { get; set; }
        public string PlantDef { get; set; }
        public bool? PlantAllowed { get; set; }
        public string AnimalDef { get; set; }
        public bool? AnimalAllowed { get; set; }
        public bool? CutNow { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class AutoSlaughterRequest
    {
        public int? MapId { get; set; }
        public string AnimalDef { get; set; }
        public int? MaxTotal { get; set; }
        public int? MaxMales { get; set; }
        public int? MaxFemales { get; set; }
        public int? MaxMalesYoung { get; set; }
        public int? MaxFemalesYoung { get; set; }
        public bool? AllowPregnant { get; set; }
        public bool? AllowBonded { get; set; }
        public bool DryRun { get; set; }
    }
}
