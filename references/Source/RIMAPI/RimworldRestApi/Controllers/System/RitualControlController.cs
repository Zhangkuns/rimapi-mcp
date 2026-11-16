// AI-FORK: G-31 ideology ritual read, instance status and native start.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class RitualControlController
    {
        private readonly IRitualControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public RitualControlController(
            IRitualControlService service,
            AiMutationCoordinator mutations
        )
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/ideology/ritual")]
        [EndpointMetadata("Read the player ideo's rituals and any running ritual")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.Get(context.Request.QueryString["ideo_id"])
            );
        }

        [Get("/api/v1/dlc/ideology/ritual/status")]
        [EndpointMetadata(
            "Read one ritual instance by ritual_id: identity, running lord, obligations and target eligibility"
        )]
        public async Task GetStatus(HttpListenerContext context)
        {
            var query = context.Request.QueryString;
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetStatus(
                    query["ritual_id"],
                    query["map_id"],
                    query["thing_id"],
                    query["x"],
                    query["z"],
                    query["organizer_pawn_id"],
                    query["role_change_precept_id"],
                    query["obligation_id"],
                    query["run_id"],
                    query["ideo_id"]
                )
            );
        }

        [Post("/api/v1/dlc/ideology/ritual/start")]
        [EndpointMetadata(
            "Start one ritual instance through its native behavior.TryExecuteOn and read back the lord"
        )]
        public async Task Start(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RitualStartRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Start(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/ritual/leave")]
        [EndpointMetadata(
            "Reproduce the native Leave ritual gizmo for one owned pawn of one exact recorded run"
        )]
        public async Task Leave(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RitualLeaveRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Leave(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/ritual/cancel")]
        [EndpointMetadata("Request native cancellation of one exact recorded ritual run")]
        public async Task Cancel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RitualCancelRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Cancel(body), Applied);
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
