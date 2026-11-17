using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;
using RIMAPI.Http;

namespace RIMAPI.Controllers
{
    public class StorageSettingsController
    {
        private readonly IStorageSettingsService _service;
        private readonly AiMutationCoordinator _mutations;

        public StorageSettingsController(IStorageSettingsService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/storage/settings")]
        [EndpointMetadata("Read complete native storage settings for a stockpile zone or storage building")]
        public async Task Get(HttpListenerContext context)
        {
            int? zone = context.Request.QueryString["zone_id"] == null ? (int?)null
                : RequestParser.GetIntParameter(context, "zone_id");
            int? map = context.Request.QueryString["map_id"] == null ? (int?)null
                : RequestParser.GetIntParameter(context, "map_id");
            await context.SendJsonResponse(_service.Get(context.Request.QueryString["thing_id"], zone, map));
        }

        [Post("/api/v1/storage/settings")]
        [EndpointMetadata("Patch, replace, allow/disallow all or copy stockpile/building storage settings; manage building groups")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<StorageSettingsRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
