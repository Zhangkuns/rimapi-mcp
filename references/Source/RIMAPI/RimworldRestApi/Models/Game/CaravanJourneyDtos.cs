namespace RIMAPI.Models
{
    public class CaravanJourneyRequest
    {
        public int CaravanId { get; set; }
        public string Action { get; set; }
        public int? TargetTile { get; set; }
        public int? WorldObjectId { get; set; }
        public string ArrivalAction { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
