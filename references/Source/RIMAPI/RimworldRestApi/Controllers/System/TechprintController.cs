using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class TechprintController
    {
        private readonly ITechprintService _service;
        private readonly AiMutationCoordinator _mutations;

        public TechprintController(ITechprintService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/royalty/techprint")]
        [EndpointMetadata("Read techprints and research benches on a map")]
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

        [Post("/api/v1/dlc/royalty/techprint/apply")]
        [EndpointMetadata("Order the native ApplyTechprint job at a research bench")]
        public async Task Apply(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TechprintApplyRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Apply(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }
    }
}
