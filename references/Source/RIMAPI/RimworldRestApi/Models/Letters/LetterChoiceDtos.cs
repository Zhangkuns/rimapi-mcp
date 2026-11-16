namespace RIMAPI.Models
{
    public sealed class LetterChoiceRequest
    {
        public int? LetterId { get; set; }
        public string OptionId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
