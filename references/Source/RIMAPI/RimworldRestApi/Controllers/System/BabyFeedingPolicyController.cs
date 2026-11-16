using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class BabyFeedingPolicyController
    {
        private readonly IBabyFeedingPolicyService _service;
        private readonly AiMutationCoordinator _mutations;

        public BabyFeedingPolicyController(
            IBabyFeedingPolicyService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/baby/feeding")]
        [EndpointMetadata("Read a baby's autofeeders and feeding policy")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _service.Get(context.Request.QueryString["baby_id"]));
        }

        [Post("/api/v1/dlc/biotech/baby/feeding")]
        [EndpointMetadata("Set a baby's autofeeder mode or allowed food")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BabyFeedingPolicyRequest>();
            await _mutations.SendAsync(
                context, body, body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }
    }
}
