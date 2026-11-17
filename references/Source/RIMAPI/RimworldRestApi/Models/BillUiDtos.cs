namespace RIMAPI.Models
{
    public class BillUiOptions
    {
        public string WorkerRestriction { get; set; }
        public int? PawnRestrictionId { get; set; }
        public BillCountSource CountSource { get; set; }
        public BillProductStyle ProductStyle { get; set; }
    }

    public sealed class BillCountSource
    {
        public string Kind { get; set; }
        public int? Id { get; set; }
    }

    public sealed class BillProductStyle
    {
        public string Mode { get; set; }
        public string StyleDef { get; set; }
        public int? GraphicIndex { get; set; }
    }

    public sealed class BillConfigureRequest : BillUiOptions
    {
        public int BuildingId { get; set; }
        public int BillId { get; set; }
        public bool DryRun { get; set; }
    }
}
