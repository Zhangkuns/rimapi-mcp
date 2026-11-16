using System;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.BaseControllers
{
    public sealed class UiTargetController
    {
        private readonly IUiTargetService _service;
        private readonly AiMutationCoordinator _main;
        public UiTargetController(IUiTargetService service, AiMutationCoordinator main) { _service = service; _main = main; }

        [Get("/api/v1/ui/targets/resolve")]
        [EndpointMetadata("Project a map cell, spawned Thing, world tile or world object into physical client pixels; observe visibility, UI occlusion and overlapping objects. Does not click.")]
        public async Task Resolve(HttpListenerContext context)
        {
            var q = context.Request.QueryString;
            try
            {
                var r = new UiTargetRequest { Kind = q["kind"], MapId = Integer(q["map_id"]), ThingId = Integer(q["thing_id"]),
                    X = Integer(q["x"]), Z = Integer(q["z"]), Tile = Integer(q["tile"]), Layer = Integer(q["layer"]), WorldObjectId = Integer(q["world_object_id"]) };
                await context.SendJsonResponse(_service.Resolve(r, q["context_id"]));
            }
            catch (ArgumentException e) { await context.SendJsonResponse(ApiResult<UiTargetResolvedDto>.Fail(e.Message, ApiErrorCode.BadRequest)); }
        }

        [Post("/api/v1/ui/targets/focus")]
        [EndpointMetadata("Move the native camera to a spatial target without selecting it or invoking gameplay callbacks. Resolve again after the next frame before input.")]
        public async Task Focus(HttpListenerContext context)
        {
            var r = await context.Request.ReadBodyAsync<UiTargetRequest>();
            await _main.RunOnMain(() => context.SendJsonResponse(_service.Focus(r)));
        }

        private static int? Integer(string value)
        {
            if (value == null) return null;
            if (!int.TryParse(value, out var n)) throw new ArgumentException("Target coordinates and IDs must be integers");
            return n;
        }
    }
}
