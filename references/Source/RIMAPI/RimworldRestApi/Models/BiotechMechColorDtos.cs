namespace RIMAPI.Models
{
    /// <summary>
    /// BT-R10: the player faction's mech accent color, written exactly like
    /// MainTabWindow_Mechs' Dialog_ChooseColor callback.
    /// </summary>
    public sealed class MechColorRequest
    {
        /// <summary>#RRGGBB or RRGGBB.</summary>
        public string Hex { get; set; }
        public bool DryRun { get; set; }
    }
}
