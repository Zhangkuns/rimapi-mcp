// G38 normal player psychic rituals.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Services;
using RIMAPI.Models;

namespace RIMAPI.Controllers
{
    public class PsychicRitualControlController
    {
        private readonly IPsychicRitualControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public PsychicRitualControlController(IPsychicRitualControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/anomaly/psychic-ritual")]
        [EndpointMetadata("Read psychic ritual defs and any running psychic ritual")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response, _service.Get());
        }

        [Get("/api/v1/dlc/anomaly/psychic-ritual/status")]
        [EndpointMetadata("Read psychic ritual eligibility, role candidates, exact running and terminal identities")]
        public async Task Status(HttpListenerContext context)
        {
            var q = context.Request.QueryString;
            await ResponseBuilder.SendApiResult(context.Response, _service.GetStatus(q["def_name"], q["map_id"], q["thing_id"], q["run_id"]));
        }

        [Post("/api/v1/dlc/anomaly/psychic-ritual/start")]
        [EndpointMetadata("Start a supported normal player psychic ritual through native assignments and MakeNewLord")]
        public async Task Start(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PsychicRitualStartRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Start(body), Applied);
        }

        private static bool Applied(ApiResult<Dictionary<string, object>> result)
        {
            object applied;
            return result.Data != null && result.Data.TryGetValue("applied", out applied) && applied is bool && (bool)applied;
        }

        [Post("/api/v1/dlc/anomaly/psychic-ritual/cancel")]
        [EndpointMetadata("Cancel one exact API psychic ritual run through the native cancellation method")]
        public async Task Cancel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PsychicRitualCancelRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Cancel(body), Applied);
        }
    }
}
