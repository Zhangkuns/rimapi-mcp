using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>
    /// Native player faction / settlement naming (Dialog_NamePlayerFaction,
    /// Dialog_NamePlayerSettlement and the combined new-game dialog).
    /// </summary>
    public class GameNamesRequestDto
    {
        public string FactionName { get; set; }
        public int? SettlementId { get; set; }
        public string SettlementName { get; set; }
        public bool DryRun { get; set; }
    }
}
