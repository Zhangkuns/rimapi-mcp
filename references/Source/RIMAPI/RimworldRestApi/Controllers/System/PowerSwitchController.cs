using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PowerSwitchController
    {
        private readonly IPowerSwitchService _service;
        private readonly AiMutationCoordinator _mutations;

        public PowerSwitchController(IPowerSwitchService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/building/power-switch")]
        [EndpointMetadata("Read native flick switch/desired state and power net")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _service.Get(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/building/power-switch")]
        [EndpointMetadata("Set the native desired flick state, cancel it, or reconnect power")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PowerSwitchRequest>();
            await _mutations.SendAsync(
                context, body, body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }
    }
}
