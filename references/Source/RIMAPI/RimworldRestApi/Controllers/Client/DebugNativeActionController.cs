using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.BaseControllers
{
    public sealed class DebugNativeActionController
    {
        private readonly IDebugNativeActionService _service;
        private readonly AiMutationCoordinator _main;
        public DebugNativeActionController(IDebugNativeActionService service, AiMutationCoordinator main)
        { _service = service; _main = main; }

        [Get("/api/v1/debug/native/actions")]
        [EndpointMetadata("List allowlisted direct native developer actions, operands and existing equivalent APIs. Does not open menus or use Cua.")]
        public async Task Catalog(HttpListenerContext context)
            => await _main.RunOnMain(() => context.SendJsonResponse(_service.Catalog()));

        [Get("/api/v1/storage/contents")]
        [EndpointMetadata("Read the native cells, stack IDs and free stack slots of one shelf or stockpile on a map.")]
        public async Task StorageContents(HttpListenerContext context)
        {
            var mapId = RequestParser.GetIntParameter(context, "map_id");
            var zoneId = context.Request.QueryString["zone_id"] == null ? (int?)null
                : RequestParser.GetIntParameter(context, "zone_id");
            var thingId = context.Request.QueryString["thing_id"];
            await _main.RunOnMain(() => context.SendJsonResponse(_service.StorageContents(mapId, thingId, zoneId)));
        }

        [Get("/api/v1/storage/shelf-capacity")]
        [EndpointMetadata("Read the current save's stack limit for all ordinary two-cell shelves.")]
        public async Task ShelfCapacity(HttpListenerContext context)
            => await _main.RunOnMain(() => context.SendJsonResponse(_service.ShelfCapacity()));

        [Post("/api/v1/debug/native/execute")]
        [EndpointMetadata("Execute an allowlisted typed native developer action on the main thread. Requires developer mode; supports dry_run and game-memory idempotency.")]
        public async Task Execute(HttpListenerContext context)
        {
            var request = await context.Request.ReadBodyAsync<DebugNativeActionRequest>();
            await _main.SendAsync(context, request, request?.DryRun == true,
                () => _service.Execute(request), Applied);
        }

        private static bool Applied(ApiResult<Dictionary<string, object>> result)
            => result.Data != null && result.Data.TryGetValue("applied", out var value) && value is bool b && b;
    }
}
