namespace RIMAPI.Models
{
    /// <summary>S-02: native reloadable gear and ammo consumption.</summary>
    public sealed class PawnReloadRequest
    {
        public int PawnId { get; set; }
        public string Action { get; set; }
        public int? GearId { get; set; }
        public bool DryRun { get; set; }
    }
}
