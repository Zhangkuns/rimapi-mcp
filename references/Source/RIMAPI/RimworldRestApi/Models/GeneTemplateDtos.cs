namespace RIMAPI.Models
{
    /// <summary>BT-R04: native custom xenogerm template save/load.</summary>
    public sealed class GeneTemplateSaveRequest
    {
        public int? MapId { get; set; }
        public string AssemblerId { get; set; }
        public string Name { get; set; }
        public string IconDef { get; set; }
        public System.Collections.Generic.List<string> GenepackIds { get; set; }
        public bool DryRun { get; set; }
    }

    public sealed class GeneTemplateLoadRequest
    {
        public int? MapId { get; set; }
        public string AssemblerId { get; set; }
        public int TemplateIndex { get; set; }
    }
}
