// AI-FORK: GET /api/v1/capabilities — what this install can do.
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class CapabilitiesController
    {
        private readonly ICapabilitiesService _capabilitiesService;

        public CapabilitiesController(ICapabilitiesService capabilitiesService)
        {
            _capabilitiesService = capabilitiesService;
        }

        [Get("/api/v1/capabilities")]
        [EndpointMetadata("AI fork: installed DLC, endpoint inventory and def-type counts")]
        public async Task GetCapabilities(HttpListenerContext context)
        {
            var result = _capabilitiesService.GetCapabilities();
            await context.SendJsonResponse(result);
        }
    }
}
