// AI-FORK: DLC royalty control DTOs.
namespace RIMAPI.Models
{
    public class PsycastRequest
    {
        public string PawnId { get; set; }
        public string AbilityDef { get; set; }
        public bool DryRun { get; set; }
    }

    public class MeditationRequest
    {
        public string PawnId { get; set; }
        public float? TargetPsyfocus { get; set; }
        public bool DryRun { get; set; }
    }

    public class PsycastCastRequest : PsycastRequest
    {
        public string TargetPawnId { get; set; }
        public PositionDto Cell { get; set; }

        /// <summary>ThingID of an item target for abilities that target things.</summary>
        public string ThingId { get; set; }

        /// <summary>Selected destination cell for two-stage abilities such as Skip.</summary>
        public PositionDto DestCell { get; set; }

        /// <summary>Native cast jobs cost psyfocus; the caller must confirm it.</summary>
        public bool Confirm { get; set; }
    }
}
