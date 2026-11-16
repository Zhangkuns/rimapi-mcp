using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class FoodPolicyController
    {
        private readonly IFoodPolicyService _service;
        private readonly AiMutationCoordinator _mutations;

        public FoodPolicyController(IFoodPolicyService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/food/policy")]
        [EndpointMetadata("List native food policies with default/in-use state")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(_service.Get());
        }

        [Get("/api/v1/food/policy/check")]
        [EndpointMetadata("Check one food def against a policy filter")]
        public async Task Check(HttpListenerContext context)
        {
            await context.SendJsonResponse(_service.Check(
                context.Request.QueryString["policy_id"],
                context.Request.QueryString["thing_def"],
                context.Request.QueryString["special_filter"]));
        }

        [Post("/api/v1/food/policy")]
        [EndpointMetadata("Create a native food policy, optionally copied")]
        public async Task Create(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FoodPolicyCreateRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Create(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/food/policy/default")]
        [EndpointMetadata("Move a food policy to the default slot")]
        public async Task SetDefault(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FoodPolicyIdRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetDefault(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/food/policy/delete")]
        [EndpointMetadata("Delete a native food policy if unused")]
        public async Task Delete(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FoodPolicyIdRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Delete(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/food/policy/filter")]
        [EndpointMetadata("Allow/disallow one food in a native policy")]
        public async Task SetFilter(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FoodPolicyFilterRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetFilter(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/pawn/food-policy")]
        [EndpointMetadata("Assign a native food policy to a pawn")]
        public async Task Assign(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnFoodPolicyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Assign(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
