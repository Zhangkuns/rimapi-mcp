using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class PsychicRitualCancelRequest
    {
        public string RunId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class PsychicRitualStartRequest
    {
        public string DefName { get; set; }
        public int? MapId { get; set; }
        public string ThingId { get; set; }
        // Native PsychicRitualRoleDef.defName -> exact Pawn ThingIDs. No implicit
        // auto-assignment: an omitted optional role remains empty.
        public Dictionary<string, List<string>> Roles { get; set; }
        public bool DryRun { get; set; }
    }
}
