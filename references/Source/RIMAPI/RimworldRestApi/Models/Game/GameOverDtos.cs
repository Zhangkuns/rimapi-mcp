using System.Collections.Generic;

namespace RIMAPI.Models
{
    public sealed class GameOverWandererRequest
    {
        public List<string> PawnIds { get; set; }
        public bool Confirm { get; set; } = true;
    }
}
