using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class OdysseyRenameController
    {
        private readonly IRenameableControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public OdysseyRenameController(
            IRenameableControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/odyssey/renameable")]
        [EndpointMetadata("Read the native IRenameable label of a grav engine or shuttle")]
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
                _service.GetRenameable(mapId, q["thing_id"]));
        }

        [Post("/api/v1/dlc/odyssey/renameable")]
        [EndpointMetadata("Set the native IRenameable label with the dialog's name rule")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RenameableRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Rename(body),
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
