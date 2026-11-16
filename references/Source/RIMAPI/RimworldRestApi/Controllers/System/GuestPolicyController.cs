using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class GuestPolicyController
    {
        private readonly IGuestPolicyService _service;
        private readonly AiMutationCoordinator _mutations;

        public GuestPolicyController(IGuestPolicyService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/pawn/guest-policy")]
        [EndpointMetadata("Read prisoner/slave interaction policy")]
        public async Task Get(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.Get(context.Request.QueryString["pawn_id"]));

        [Post("/api/v1/pawn/guest-policy")]
        [EndpointMetadata("Set prisoner/slave interaction policy")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GuestPolicyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
        [Get("/api/v1/guest/policy")]
        [EndpointMetadata("Read native prisoner/slave continuous policy, eligibility and scheduling state")]
        public async Task GetNativeGuestPolicy(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response,
                _service.GetNative(context.Request.QueryString["pawn_id"]));
        }

        [Post("/api/v1/guest/policy")]
        [EndpointMetadata("Set/start/stop native prisoner or slave continuous interaction policy")]
        public async Task GuestPolicyAction(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GuestPolicyActionRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Action(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

    }
}
