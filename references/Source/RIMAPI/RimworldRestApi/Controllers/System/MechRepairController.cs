using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class MechRepairController
    {
        private readonly IMechRepairControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public MechRepairController(
            IMechRepairControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/mech/auto-repair")]
        [EndpointMetadata("Read a mech's native autoRepair policy")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _service.Get(context.Request.QueryString["pawn_id"]));
        }

        [Post("/api/v1/dlc/biotech/mech/auto-repair")]
        [EndpointMetadata("Set a mech's native autoRepair policy")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechAutoRepairRequest>();
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
