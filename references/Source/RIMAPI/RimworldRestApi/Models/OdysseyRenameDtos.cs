namespace RIMAPI.Models
{
    /// <summary>
    /// OD-R07: the normal IRenameable label used by gravship and passenger
    /// shuttle naming dialogs. Legacy display-only names are not written anywhere
    /// else, and the native dialog's 1..64/no-special-character rule is kept.
    /// </summary>
    public sealed class RenameableRequest
    {
        public string ThingId { get; set; }
        public string Name { get; set; }
        public bool DryRun { get; set; }
    }
}
