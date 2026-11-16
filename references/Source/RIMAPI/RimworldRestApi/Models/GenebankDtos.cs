namespace RIMAPI.Models
{
    /// <summary>
    /// BT-R03: native CompGenepackContainer/Genepack controls. auto_load toggles
    /// the bank and clears its pending queue like the native contents tab; load
    /// is "pack" (queue the exact pack: targetContainer + leftToLoad) or "cancel"
    /// (native uncheck); pack_auto_load sets the per-genepack AutoLoad policy;
    /// eject is "all" or "pack".
    /// </summary>
    public sealed class GenebankRequest
    {
        public int? MapId { get; set; }
        public string BankId { get; set; }
        public bool? AutoLoad { get; set; }
        public string Eject { get; set; }
        public string PackId { get; set; }
        public string Load { get; set; }
        public bool? PackAutoLoad { get; set; }
        public bool DryRun { get; set; }
    }
}
