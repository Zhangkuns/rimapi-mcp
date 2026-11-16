using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class CaravanManageItemRequest
    {
        public string ThingId { get; set; }
        public int Count { get; set; } = 1;
    }

    public class CaravanManageRequest
    {
        public int CaravanId { get; set; }
        public string Action { get; set; }
        public List<string> PawnIds { get; set; }
        public List<CaravanManageItemRequest> Items { get; set; }
        public int? OtherCaravanId { get; set; }
        public string PawnId { get; set; }
        public string ThingId { get; set; }
        public bool? BeCarriedByCaravanIfSick { get; set; }
        public string BabyFoodDef { get; set; }
        public bool? BabyFoodAllowed { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
