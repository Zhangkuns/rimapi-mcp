using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class PreferencesRequest
    {
        public Dictionary<string, object> Values { get; set; }
        public DisplayResolutionRequest Resolution { get; set; }
        public string Language { get; set; }
        public bool ConfirmDisplay { get; set; }
        public bool ResetLearningHelper { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class DisplayResolutionRequest
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public bool? Fullscreen { get; set; }
    }

    public sealed class PlaySettingsRequest
    {
        public Dictionary<string, bool> Values { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class KeyBindingsRequest
    {
        public List<KeyBindingChange> Bindings { get; set; }
        public bool ResetAll { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class KeyBindingChange
    {
        public string DefName { get; set; }
        public string Slot { get; set; }
        public string KeyCode { get; set; }
        public bool Reset { get; set; }
    }
}
