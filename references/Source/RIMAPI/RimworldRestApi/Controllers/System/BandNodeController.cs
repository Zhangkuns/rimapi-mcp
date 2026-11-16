using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class BandNodeController
    {
        private readonly IBandNodeControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public BandNodeController(
            IBandNodeControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/band-node")]
        [EndpointMetadata("Read player band nodes: tuned/tuning pawn and remaining ticks")]
        public async Task Get(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await context.SendJsonResponse(
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await context.SendJsonResponse(_service.Get(mapId));
        }

        [Post("/api/v1/dlc/biotech/band-node/tune")]
        [EndpointMetadata("Tune/retune a band node to a mechanitor (native TuneTo)")]
        public async Task Tune(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BandNodeTuneRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Tune(body),
                Applied);
        }

        private static bool Applied(ApiResult<Dictionary<string, object>> result)
        {
            object applied;
            return result.Data != null
                && result.Data.TryGetValue("applied", out applied)
                && applied is bool && (bool)applied;
        }
    }
}
