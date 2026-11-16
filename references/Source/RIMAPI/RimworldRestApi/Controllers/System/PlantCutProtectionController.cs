using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PlantCutProtectionController
    {
        private readonly IPlantCutProtectionService _service;
        private readonly AiMutationCoordinator _mutations;

        public PlantCutProtectionController(
            IPlantCutProtectionService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/plant/cut-protection")]
        [EndpointMetadata("Read a tree's native prevent-cutting flag and designation")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _service.Get(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/plant/cut-protection")]
        [EndpointMetadata("Toggle a tree's native prevent-cutting flag")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PlantCutProtectionRequest>();
            await _mutations.SendAsync(
                context, body, body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }
    }
}
