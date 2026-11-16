using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class OdysseyHackController
    {
        private readonly IHackableControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public OdysseyHackController(
            IHackableControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/odyssey/hackable")]
        [EndpointMetadata("Read one native CompHackable and the WorkGiver_Hack gate")]
        public async Task Get(HttpListenerContext context)
        {
            var q = context.Request.QueryString;
            int? mapId = null;
            int parsed;
            if (!string.IsNullOrWhiteSpace(q["map_id"]))
            {
                if (!int.TryParse(q["map_id"], out parsed))
                {
                    await context.SendJsonResponse(
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await context.SendJsonResponse(
                _service.GetHackable(mapId, q["thing_id"], q["pawn_id"]));
        }

        [Post("/api/v1/dlc/odyssey/hackable/auto")]
        [EndpointMetadata("Set the native auto-hack policy on one CompHackable")]
        public async Task SetAuto(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<HackableAutoRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetAutoHack(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/odyssey/locked")]
        [EndpointMetadata("Read locked ancient doors, crates and mechanoid relay/stabilizer state")]
        public async Task GetLocked(HttpListenerContext context)
        {
            var q = context.Request.QueryString;
            int? mapId = null;
            int parsed;
            if (!string.IsNullOrWhiteSpace(q["map_id"]))
            {
                if (!int.TryParse(q["map_id"], out parsed))
                {
                    await context.SendJsonResponse(
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await context.SendJsonResponse(
                _service.GetLocked(mapId, q["thing_id"], q["pawn_id"]));
        }

        [Post("/api/v1/dlc/odyssey/locked")]
        [EndpointMetadata("Order the native Open or jammed-door InteractThing job on a locked thing")]
        public async Task LockedAction(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<LockedActionRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.LockedAction(body),
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
