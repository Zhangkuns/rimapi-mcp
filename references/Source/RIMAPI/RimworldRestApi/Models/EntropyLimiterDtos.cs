namespace RIMAPI.Models
{
    /// <summary>RY-R01: native PsychicEntropyGizmo limitEntropyAmount toggle.</summary>
    public sealed class EntropyLimiterRequest
    {
        public string PawnId { get; set; }
        public bool Enabled { get; set; }
        public bool DryRun { get; set; }
    }
}
