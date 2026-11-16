using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>
    /// C-15: drafted combat policy and attack-queue readback. The request only
    /// applies native switches; it never orders an arbitrary target or edits the
    /// job queue.
    /// </summary>
    public class CombatPolicyRequestDto
    {
        public int PawnId { get; set; }
        public bool? Drafted { get; set; }
        public bool? FireAtWill { get; set; }
        public string HostilityResponse { get; set; }
        public bool Stop { get; set; }
        public bool DryRun { get; set; }
    }

    public class CombatTargetDto
    {
        public int? ThingId { get; set; }
        public string ThingIdString { get; set; }
        public string DefName { get; set; }
        public int? X { get; set; }
        public int? Z { get; set; }
    }

    public class CombatJobDto
    {
        public string DefName { get; set; }
        public CombatTargetDto TargetA { get; set; }
        public CombatTargetDto TargetB { get; set; }
        public bool PlayerForced { get; set; }
    }
}
