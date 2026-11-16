namespace RIMAPI.Models
{
    /// <summary>
    /// OD-R03: the animal forage/dig checkboxes on PawnColumnWorker_AnimalForage
    /// and PawnColumnWorker_AnimalDig. Null leaves that field unchanged.
    /// </summary>
    public sealed class AnimalWorkRequest
    {
        public string PawnId { get; set; }
        public bool? Forage { get; set; }
        public bool? Dig { get; set; }
        public bool DryRun { get; set; }
    }
}
