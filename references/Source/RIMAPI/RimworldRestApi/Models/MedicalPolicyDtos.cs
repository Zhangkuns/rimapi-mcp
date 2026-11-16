namespace RIMAPI.Models
{
    public sealed class MedicalDefaultsRequest
    {
        public string Colonist { get; set; }
        public string Prisoner { get; set; }
        public string Slave { get; set; }
        public string TamedAnimal { get; set; }
        public string FriendlyFaction { get; set; }
        public string NeutralFaction { get; set; }
        public string HostileFaction { get; set; }
        public string NoFaction { get; set; }
        public string Wildlife { get; set; }
        public string Entities { get; set; }
        public string Ghouls { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class PawnMedicalSettingsRequest
    {
        public string PawnId { get; set; }
        public string MedCare { get; set; }
        public bool? SelfTend { get; set; }
        public bool DryRun { get; set; }
    }
}
