using System.Collections.Generic;
namespace RIMAPI.Models
{
    public sealed class TemperatureControlRequest
    {
        public string ThingId { get; set; }
        public float? TargetTemperature { get; set; }
        public string Action { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class FuelControlRequest
    {
        public string ThingId { get; set; }
        public float? TargetFuelLevel { get; set; }
        public bool? AllowAutoRefuel { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class FuelRefuelRequest
    {
        public string ThingId { get; set; }
        public string PawnId { get; set; }
        public bool Forced { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class TargetCellDto
    {
        public int X { get; set; }
        public int Z { get; set; }
    }

    public sealed class TurretControlRequest
    {
        public string ThingId { get; set; }
        public string Action { get; set; }
        public string TargetPawnId { get; set; }
        public TargetCellDto Target { get; set; }
        public string ShellDef { get; set; }
        public bool? Enabled { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class ExplosiveControlRequest
    {
        public string ThingId { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class CancelBuildRequest
    {
        public List<string> ThingIds { get; set; }
        public string ThingId { get; set; }
        public int? X { get; set; }
        public int? Z { get; set; }
        public int? Rotation { get; set; }
        public bool DryRun { get; set; }
    }
}
