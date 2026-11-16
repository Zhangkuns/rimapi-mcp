using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
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
