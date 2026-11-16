using System.Collections.Generic;

namespace RIMAPI.Models
{
    /// <summary>C-17: normal pickup/drop/dress-other actions and exact holder readback.</summary>
    public sealed class PawnEquipmentRequest
    {
        public int PawnId { get; set; }
        public string Action { get; set; }
        public int? ItemId { get; set; }
        public int? Count { get; set; }
        public int? ThingId { get; set; }
        public int? TargetPawnId { get; set; }
        public int? ApparelId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
