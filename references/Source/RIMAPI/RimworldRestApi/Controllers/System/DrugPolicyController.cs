using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class DrugPolicyController
    {
        private readonly IDrugPolicyService _service;
        private readonly AiMutationCoordinator _mutations;

        public DrugPolicyController(IDrugPolicyService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/drug/policy")]
        [EndpointMetadata("List native drug policies")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(_service.Get());
        }

        [Get("/api/v1/drug/policy/entry")]
        [EndpointMetadata("Read one native drug-policy entry")]
        public async Task GetEntry(HttpListenerContext context)
        {
            await context.SendJsonResponse(_service.GetEntry(
                context.Request.QueryString["policy_id"],
                context.Request.QueryString["drug_def"]));
        }

        [Post("/api/v1/drug/policy")]
        [EndpointMetadata("Create a native drug policy, optionally from a preset/copy")]
        public async Task Create(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DrugPolicyCreateRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Create(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/drug/policy/default")]
        [EndpointMetadata("Move a drug policy to the default slot")]
        public async Task SetDefault(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DrugPolicyIdRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetDefault(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/drug/policy/delete")]
        [EndpointMetadata("Delete a native drug policy if unused")]
        public async Task Delete(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DrugPolicyIdRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Delete(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/drug/policy/entry")]
        [EndpointMetadata("Set one native drug-policy entry rule")]
        public async Task SetEntry(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DrugPolicyEntryRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetEntry(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/pawn/drug-policy")]
        [EndpointMetadata("Assign a native drug policy to a pawn")]
        public async Task Assign(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnDrugPolicyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Assign(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
