namespace RIMAPI.Models
{
    public class DialogDecisionRequest
    {
        public string DialogId { get; set; }
        public string OptionId { get; set; }
        public bool Confirm { get; set; }
        public bool DryRun { get; set; }
    }
}
