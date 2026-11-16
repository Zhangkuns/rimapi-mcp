namespace RIMAPI.Models
{
    /// <summary>BT-R17: native CompMechRepairable autoRepair toggle.</summary>
    public sealed class MechAutoRepairRequest
    {
        public string PawnId { get; set; }
        public bool Enabled { get; set; }
        public bool DryRun { get; set; }
    }
}
