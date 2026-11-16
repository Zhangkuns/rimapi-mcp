using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

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
        [EndpointMetadata("Read native storage priority/filter/group")]
        public async Task Get(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.Get(context.Request.QueryString["thing_id"]));

        [Post("/api/v1/storage/settings")]
        [EndpointMetadata("Set storage priority/filter and create/enroll a group")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<StorageSettingsRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
