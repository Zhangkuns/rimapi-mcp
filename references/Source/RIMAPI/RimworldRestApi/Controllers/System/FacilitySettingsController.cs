using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class FacilitySettingsController
    {
        private readonly IFacilitySettingsService _service;
        private readonly AiMutationCoordinator _mutations;

        public FacilitySettingsController(IFacilitySettingsService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/building/facility")]
        [EndpointMetadata("Read door/gather/light/trap/scanner facility settings")]
        public async Task Get(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.Get(context.Request.QueryString["thing_id"]));

        [Post("/api/v1/building/facility")]
        [EndpointMetadata("Set facility hold-open/gather/glow/trap/scanner target")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FacilitySettingsRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
