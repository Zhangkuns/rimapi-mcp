using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class RewardPrefsController
    {
        private readonly IRewardPrefsService _service;
        private readonly AiMutationCoordinator _mutations;

        public RewardPrefsController(
            IRewardPrefsService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/royalty/reward-prefs")]
        [EndpointMetadata("Read faction royal-favor and goodwill reward preferences")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(_service.Get());
        }

        [Post("/api/v1/dlc/royalty/reward-prefs")]
        [EndpointMetadata("Set a faction's native reward preference flags")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RewardPrefsRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }
    }
}
