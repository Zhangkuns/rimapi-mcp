// AI-FORK: DTOs for GET /api/v1/capabilities (agent discovery).
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class CapabilitiesDto
    {
        public string Fork { get; set; }
        public string ApiVersion { get; set; }
        public string GameVersion { get; set; }
        public Dictionary<string, bool> Dlc { get; set; }
        public bool AuthEnabled { get; set; }
        public List<CapabilityEndpointDto> Endpoints { get; set; }
        public DefTypeSummaryDto DefTypes { get; set; }
    }

    public class CapabilityEndpointDto
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        /// <summary>True when the endpoint can change game state.</summary>
        public bool Write { get; set; }

        /// <summary>observe / policy / command / direct (heuristic, see LayerSource).</summary>
        public string Layer { get; set; }

        /// <summary>none / game_ai / pawn_ai / immediate (heuristic).</summary>
        public string Executor { get; set; }

        /// <summary>Whether the endpoint supports dry-run (new trade contracts use the JSON body).</summary>
        public bool DryRun { get; set; }
        public string DryRunMode { get; set; }
        public string Idempotency { get; set; }

        /// <summary>DLC required; contract null means no fixed requirement, heuristic null means undeclared.</summary>
        public string Dlc { get; set; }

        /// <summary>"contract" for explicitly mapped endpoints, otherwise "heuristic".</summary>
        public string LayerSource { get; set; }
    }

    public class DefTypeSummaryDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
    }
}
