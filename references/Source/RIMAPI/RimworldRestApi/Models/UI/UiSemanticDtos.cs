using System;
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class UiRectDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public sealed class UiControlDto
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public bool? Checked { get; set; }
        public string Source { get; set; }
        public string WindowType { get; set; }
        public string WindowId { get; set; }
        public UiRectDto Bounds { get; set; }
        public UiRectDto VisibleBounds { get; set; }
        public bool Enabled { get; set; }
        public bool Clickable { get; set; }
        public string DisabledReason { get; set; }
        public int DrawOrder { get; set; }
        public int ZOrder { get; set; }
    }

    public sealed class UiSnapshotDto
    {
        public int ProcessId { get; set; }
        public bool Ready { get; set; }
        public string SnapshotId { get; set; }
        public string Scene { get; set; }
        public int Frame { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public float UiScale { get; set; }
        public DateTime CapturedAtUtc { get; set; }
        public int TotalControls { get; set; }
        public bool Truncated { get; set; }
        public List<UiControlDto> Controls { get; set; } = new List<UiControlDto>();
        public string[] Coverage { get; set; } = new string[0];
        public string[] Limitations { get; set; } = new string[0];
    }

    public sealed class UiResolvedControlDto
    {
        public int ProcessId { get; set; }
        public string SnapshotId { get; set; }
        public int Frame { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public UiControlDto Control { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
