namespace RIMAPI.Models
{
    public class ClaimThingRequestDto
    {
        public string ThingId { get; set; }
        public bool DryRun { get; set; }
    }

    public class AdoptPawnRequestDto
    {
        public string PawnId { get; set; }
        public bool DryRun { get; set; }
    }

    public class MineVeinRequestDto
    {
        public int MapId { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
        public bool Cancel { get; set; }
        public bool DryRun { get; set; }
    }
}
