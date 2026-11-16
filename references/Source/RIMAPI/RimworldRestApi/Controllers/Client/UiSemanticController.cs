using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.BaseControllers
{
    public sealed class UiSemanticController
    {
        private readonly IUiSemanticService _service;
        public UiSemanticController(IUiSemanticService service) { _service = service; }

        [Get("/api/v1/ui/controls")]
        [EndpointMetadata("Observe native IMGUI controls, visible bounds and window ownership. First read warms a five-second observation lease; ready=false until Repaint.")]
        public async Task GetControls(HttpListenerContext context)
        {
            var q = context.Request.QueryString;
            int limit = 500;
            bool includeDisabled = true;
            if ((q["limit"] != null && !int.TryParse(q["limit"], out limit))
                || (q["include_disabled"] != null && !bool.TryParse(q["include_disabled"], out includeDisabled)))
            {
                await context.SendJsonResponse(ApiResult<UiSnapshotDto>.Fail("Invalid limit or include_disabled", ApiErrorCode.BadRequest));
                return;
            }
            await context.SendJsonResponse(_service.GetControls(q["label"], q["role"], q["window"], limit, includeDisabled));
        }

        [Get("/api/v1/ui/controls/resolve")]
        [EndpointMetadata("Resolve one current, clickable UI control to physical client-area pixels; rejects stale layout. Does not click or invoke game callbacks.")]
        public async Task Resolve(HttpListenerContext context)
        {
            var q = context.Request.QueryString;
            await context.SendJsonResponse(_service.Resolve(q["snapshot_id"], q["control_id"]));
        }
    }
}
