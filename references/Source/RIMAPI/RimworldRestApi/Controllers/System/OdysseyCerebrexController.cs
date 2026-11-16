using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class OdysseyCerebrexController
    {
        private readonly ICerebrexControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public OdysseyCerebrexController(
            ICerebrexControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/odyssey/cerebrex-core")]
        [EndpointMetadata("Read the native CerebrexCore stabilizer/deactivation state")]
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
            int? thingId = null;
            if (!string.IsNullOrWhiteSpace(q["thing_id"]))
            {
                if (!int.TryParse(q["thing_id"], out parsed))
                {
                    await context.SendJsonResponse(
                        ApiResult<Dictionary<string, object>>.Fail(
                            "thing_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                thingId = parsed;
            }
            await context.SendJsonResponse(_service.GetCore(mapId, thingId));
        }

        [Post("/api/v1/dlc/odyssey/cerebrex-core/choose")]
        [EndpointMetadata("Choose destroy/scavenge/postpone on the native CerebrexCore")]
        public async Task Choose(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CerebrexChoiceRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Choose(body),
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
