using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class ReadingPolicyController
    {
        private readonly IReadingPolicyService _service;
        private readonly AiMutationCoordinator _mutations;

        public ReadingPolicyController(IReadingPolicyService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/reading/policy")]
        [EndpointMetadata("List native reading policies")]
        public async Task Get(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.Get());

        [Get("/api/v1/reading/policy/check")]
        [EndpointMetadata("Check a book/effect against a reading policy")]
        public async Task Check(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.Check(
                context.Request.QueryString["policy_id"],
                context.Request.QueryString["thing_def"],
                context.Request.QueryString["special_filter"]));

        [Post("/api/v1/reading/policy")]
        [EndpointMetadata("Create/copy a native reading policy")]
        public async Task Create(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ReadingPolicyCreateRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Create(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/reading/policy/default")]
        [EndpointMetadata("Move a reading policy to the default slot")]
        public async Task SetDefault(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ReadingPolicyIdRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetDefault(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/reading/policy/delete")]
        [EndpointMetadata("Delete a reading policy if unused")]
        public async Task Delete(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ReadingPolicyIdRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Delete(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/reading/policy/filter")]
        [EndpointMetadata("Set one reading def/special-effect filter")]
        public async Task SetFilter(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ReadingPolicyFilterRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetFilter(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/pawn/reading-policy")]
        [EndpointMetadata("Assign a native reading policy to a pawn")]
        public async Task Assign(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnReadingPolicyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Assign(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
