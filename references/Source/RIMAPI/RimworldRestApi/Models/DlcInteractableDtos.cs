namespace RIMAPI.Models
{
    /// <summary>
    /// AN-06: normal player interaction with a native CompInteractable object
    /// (obelisk trigger/deactivation, golden cube, revenant spine).
    /// </summary>
    public sealed class InteractableRequest
    {
        /// <summary>Native thingIDNumber of the interactable object.</summary>
        public int? ThingId { get; set; }

        public string PawnId { get; set; }
        public int? MapId { get; set; }

        /// <summary>
        /// Native CompInteractable.ExposeKey ("Interactor", "Deactivation", ...).
        /// Required when one thing carries several interactable components.
        /// </summary>
        public string InteractableKey { get; set; }

        /// <summary>
        /// Accepts the native destructive confirmation (golden cube lists every pawn
        /// that will berserk or be banished).
        /// </summary>
        public bool Confirm { get; set; }

        public bool DryRun { get; set; }
    }

    /// <summary>
    /// AN-06c: send the platform-held nociosphere to an exact destination cell.
    /// The native menu sets sendLocation through a cell targeter before ordering
    /// the InteractThing job; this endpoint takes that cell explicitly.
    /// </summary>
    public sealed class NociosphereSendRequest
    {
        /// <summary>Native thingIDNumber of the nociosphere pawn.</summary>
        public int? ThingId { get; set; }

        public string PawnId { get; set; }
        public int? MapId { get; set; }

        /// <summary>Exact destination cell the player would click.</summary>
        public PositionDto Cell { get; set; }

        public bool DryRun { get; set; }
    }
}
