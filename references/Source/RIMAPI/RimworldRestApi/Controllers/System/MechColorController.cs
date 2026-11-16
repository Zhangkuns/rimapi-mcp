using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class MechColorController
    {
        private readonly IMechColorControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public MechColorController(
            IMechColorControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/mech/color")]
        [EndpointMetadata("Read the player faction mech accent color")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(_service.Get());
        }

        [Post("/api/v1/dlc/biotech/mech/color")]
        [EndpointMetadata("Set the player faction mech accent color and dirty mech portraits")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechColorRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Set(body),
                Applied
            );
        }

        private static bool Applied(ApiResult<Dictionary<string, object>> result)
        {
            object applied;
            return result.Data != null
                && result.Data.TryGetValue("applied", out applied)
                && applied is bool
                && (bool)applied;
        }
    }
}
