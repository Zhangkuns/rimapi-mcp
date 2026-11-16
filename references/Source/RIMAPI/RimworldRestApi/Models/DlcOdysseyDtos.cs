namespace RIMAPI.Models
{
    /// <summary>
    /// OD-07a: normal use of a Building_OutfitStand. The native job swaps the
    /// pawn's worn apparel with the stand's held apparel (and weapon if allowed).
    /// </summary>
    public sealed class OutfitStandManageRequest
    {
        public int? MapId { get; set; }
        public int? StandId { get; set; }
        public string PawnId { get; set; }
        public string ApparelId { get; set; }
        /// <summary>set_hauling | put | eject</summary>
        public string Action { get; set; }
        public bool? Enabled { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class OutfitStandUseRequest
    {
        public string PawnId { get; set; }

        /// <summary>Native thingIDNumber of the outfit stand.</summary>
        public int? StandId { get; set; }

        public bool DryRun { get; set; }
    }
}
