namespace RIMAPI.Models
{
    /// <summary>
    /// BT-R09: native Building_Enterable.SelectPawn for a subcore scanner, including
    /// a downed/prisoner donor (the native WorkGiver supplies a carrier), and a
    /// cancel-load path that clears the selected pawn without ejecting loaded
    /// ingredients.
    /// </summary>
    public sealed class SubcoreSelectRequest
    {
        public string ScannerId { get; set; }
        public string PawnId { get; set; }
        public bool Cancel { get; set; }
        public bool DryRun { get; set; }
    }
}
