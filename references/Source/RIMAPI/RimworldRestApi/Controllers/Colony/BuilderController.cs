using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;
using RIMAPI.Http;

namespace RIMAPI.Controllers
{
    public class BuilderController
    {
        private readonly IBuilderService _builderService;
        private readonly IBuildControlService _buildControl;

        public BuilderController(IBuilderService builderService,
            IBuildControlService buildControl)
        {
            _builderService = builderService;
            _buildControl = buildControl;
        }

        [Post("/api/v1/builder/copy")]
        public async Task CopyArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CopyAreaRequestDto>();
            var result = _builderService.CopyArea(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/builder/constructions")]
        [EndpointMetadata("List all Blueprint/Frame objects on a map or intersecting rectangle, including materials and work")]
        public async Task GetConstructions(HttpListenerContext context)
        {
            int mapId = RequestParser.GetMapId(context);
            int? minX = ReadOptionalInt(context, "min_x");
            int? minZ = ReadOptionalInt(context, "min_z");
            int? maxX = ReadOptionalInt(context, "max_x");
            int? maxZ = ReadOptionalInt(context, "max_z");
            await context.SendJsonResponse(_buildControl.GetConstructions(mapId, minX, minZ, maxX, maxZ));
        }

        static int? ReadOptionalInt(HttpListenerContext context, string name) =>
            context.Request.QueryString[name] == null ? (int?)null
                : RequestParser.GetIntParameter(context, name);

        [Post("/api/v1/builder/paste")]
        public async Task PasteArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PasteAreaRequestDto>();
            var result = _builderService.PasteArea(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/builder/blueprint")]
        public async Task PlaceBlueprints(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PasteAreaRequestDto>();
            var result = _builderService.PlaceBlueprints(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/builder/check-zone")]
        public async Task CheckZone(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CheckZoneRequestDto>();
            var result = _builderService.CheckZone(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/builder/qualification")]
        [EndpointMetadata("Native build research/placement qualification")]
        public async Task GetQualification(HttpListenerContext context)
        {
            int.TryParse(context.Request.QueryString["x"], out var x);
            int.TryParse(context.Request.QueryString["z"], out var z);
            int.TryParse(context.Request.QueryString["rotation"], out var rot);
            await context.SendJsonResponse(_buildControl.GetQualification(
                context.Request.QueryString["def_name"],
                context.Request.QueryString["stuff_def"], x, z, rot));
        }

        [Post("/api/v1/builder/cancel")]
        [EndpointMetadata("Cancel native Blueprint/Frame with normal refund")]
        public async Task CancelBlueprints(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CancelBuildRequest>();
            await context.SendJsonResponse(_buildControl.CancelBlueprints(body));
        }

        [Post("/api/v1/builder/install")]
        [EndpointMetadata("Native minified install/building reinstall blueprint")]
        public async Task Install(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CancelBuildRequest>();
            await context.SendJsonResponse(_buildControl.Install(body));
        }
    }
}
