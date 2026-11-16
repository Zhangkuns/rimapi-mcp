// AI-FORK: Biotech gene endpoints. Writes go through the mutation coordinator so
// dry_run, idempotency, response freeze and operation events stay consistent.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class GeneControlController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IGeneControlService _service;

        public GeneControlController(IGeneControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/gene")]
        [EndpointMetadata("Read xenotype and endogene/xenogene lists for a colonist")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.Get(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/biotech/gene")]
        [EndpointMetadata("Add one gene to the endogene or xenogene layer")]
        public async Task Add(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneActionRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Add(body),
                Changed
            );
        }

        [Post("/api/v1/dlc/biotech/xenotype")]
        [EndpointMetadata("Apply a native xenotype change; confirm:true is required")]
        public async Task SetXenotype(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<XenotypeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetXenotype(body)
            );
        }

        [Delete("/api/v1/dlc/biotech/gene")]
        [EndpointMetadata("Remove one gene from the selected layer")]
        public async Task Remove(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneActionRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Remove(body),
                Changed
            );
        }

        private static bool Changed(ApiResult<Dictionary<string, object>> result)
        {
            object changed;
            return result.Data != null
                && result.Data.TryGetValue("changed", out changed)
                && changed is bool
                && (bool)changed;
        }
    }
}
