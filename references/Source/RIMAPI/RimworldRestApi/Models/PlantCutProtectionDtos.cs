namespace RIMAPI.Models
{
    /// <summary>S-09: native CompPlantPreventCutting toggle.</summary>
    public sealed class PlantCutProtectionRequest
    {
        public string ThingId { get; set; }
        public bool PreventCutting { get; set; }
        public bool DryRun { get; set; }
    }
}
