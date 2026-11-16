using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class QuestRewardChoiceRequest
    {
        public int PartIndex { get; set; }
        public int ChoiceIndex { get; set; }
    }

    public class QuestAcceptRequest
    {
        public int QuestId { get; set; }
        public string PawnId { get; set; }
        public List<QuestRewardChoiceRequest> Choices { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public class ArchonexusQuestRequest
    {
        public int QuestId { get; set; }
        public bool DryRun { get; set; }
    }

    public class ArchonexusSelectRequest : ArchonexusQuestRequest
    {
        // IDs come from the native dialog's grouped rows, not arbitrary Things.
        public List<string> ThingIds { get; set; }
        public bool Confirm { get; set; }
    }

    public class ArchonexusTileRequest : ArchonexusQuestRequest
    {
        public int TileId { get; set; } = -1;
        public int LayerId { get; set; }
    }

    public class ArchonexusIdeologyRequest : ArchonexusQuestRequest
    {
        // Explicitly choose to keep the existing ideologies, select an existing
        // primary ideo, or create/load an ideo from the same C-04
        // configuration vocabulary. PawnIdeos optionally converts restart pawns.
        public bool KeepIdeology { get; set; }
        public int? PrimaryIdeoId { get; set; }
        public Dictionary<string, int> PawnIdeos { get; set; }

        /// <summary>
        /// ID-R11: create a new fixed/fluid ideo during the Archonexus restart
        /// dialog. Uses the C-04 NewGameIdeoDto semantics (mode/name/structure
        /// meme/normal memes/styles), or mode=load with a listed saved .rid
        /// preset_file. Built-in preset and classic are not restart creations.
        /// </summary>
        public NewGameIdeoDto Ideo { get; set; }

        public bool Confirm { get; set; }
    }

    public class ArchonexusStudyRequest
    {
        public string StructureId { get; set; }
        public bool Enabled { get; set; }
        public bool DryRun { get; set; }
    }

    public class ArchonexusActivateRequest
    {
        public string CoreId { get; set; }
        public string PawnId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
