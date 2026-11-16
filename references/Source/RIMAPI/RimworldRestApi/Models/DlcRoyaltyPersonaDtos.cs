// AI-FORK: persona (bladelink) confirmation/equip DTOs (RY-R08).
namespace RIMAPI.Models
{
    public sealed class RoyalPersonaEquipRequest
    {
        public string PawnId { get; set; }
        public string ThingId { get; set; }

        /// <summary>Required when the native persona confirmation text is non-empty.</summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }
}
