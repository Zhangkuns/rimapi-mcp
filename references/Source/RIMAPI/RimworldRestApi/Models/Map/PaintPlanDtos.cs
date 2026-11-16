using System.Collections.Generic;
namespace RIMAPI.Models
{
    public sealed class PaintRequest
    {
        public int? X { get; set; }
        public int? Z { get; set; }
        public string ThingId { get; set; }
        public string Target { get; set; }
        public string Action { get; set; }
        public string Color { get; set; }
        public bool DryRun { get; set; }
    }
    public sealed class PlanRequest
    {
        public string Action { get; set; }
        public int? PlanId { get; set; }
        public string Label { get; set; }
        public string Color { get; set; }
        public List<TargetCellDto> Cells { get; set; }
        public TargetCellDto Offset { get; set; }
        public bool DryRun { get; set; }
    }
}
