// AI-FORK: quest generation control. Generation is previewed first; adding the quest
// to the quest manager is the irreversible step and requires confirm=true.
namespace RIMAPI.Models
{
    public class QuestRemoveRequest
    {
        public int QuestId { get; set; }
        public bool DryRun { get; set; }
    }

    public class QuestGenerateRequest
    {
        /// <summary>QuestScriptDef defName; empty picks nothing (caller must supply).</summary>
        public string ScriptDef { get; set; }

        /// <summary>Optional points budget; 0 uses the script default.</summary>
        public float Points { get; set; }

        /// <summary>Preview only: build the Quest object and report it, never add it.</summary>
        public bool DryRun { get; set; }

        /// <summary>Explicit consent to add the generated quest to the quest manager.</summary>
        public bool Confirm { get; set; }
    }
}
