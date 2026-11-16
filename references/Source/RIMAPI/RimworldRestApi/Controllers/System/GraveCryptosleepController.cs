using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class GraveCryptosleepController
    {
        private readonly IGraveCryptosleepService _service;
        private readonly AiMutationCoordinator _mutations;

        public GraveCryptosleepController(
            IGraveCryptosleepService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/building/grave")]
        [EndpointMetadata("Read a grave's native assignment and corpse")]
        public async Task GetGrave(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _service.GetGrave(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/building/grave/assign")]
        [EndpointMetadata("Assign or unassign a native grave claimant")]
        public async Task AssignGrave(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GraveAssignRequest>();
            await _mutations.SendAsync(
                context, body, body != null && body.DryRun,
                () => _service.AssignGrave(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Get("/api/v1/building/cryptosleep")]
        [EndpointMetadata("Read a cryptosleep casket's contained thing")]
        public async Task GetCasket(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _service.GetCasket(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/building/cryptosleep/carry")]
        [EndpointMetadata("Start a native CarryToCryptosleepCasket job for a downed pawn")]
        public async Task CarryToCasket(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CryptosleepCarryRequest>();
            await _mutations.SendAsync(
                context, body, body != null && body.DryRun,
                () => _service.CarryToCasket(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/building/cryptosleep/eject")]
        [EndpointMetadata("Eject a cryptosleep casket through its native EjectContents")]
        public async Task Eject(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CryptosleepEjectRequest>();
            await _mutations.SendAsync(
                context, body, body != null && body.DryRun,
                () => _service.EjectCasket(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }
    }
}
