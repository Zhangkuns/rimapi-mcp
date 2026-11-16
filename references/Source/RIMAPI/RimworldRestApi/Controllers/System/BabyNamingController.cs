using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class BabyNamingController
    {
        private readonly IBabyNamingService _service;
        private readonly AiMutationCoordinator _mutations;

        public BabyNamingController(IBabyNamingService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/pawn/baby-name")]
        [EndpointMetadata("Read a baby's naming deadline and birth letter")]
        public async Task Get(HttpListenerContext context) =>
            await context.SendJsonResponse(
                _service.Get(context.Request.QueryString["pawn_id"]));

        [Post("/api/v1/pawn/baby-name")]
        [EndpointMetadata("Complete baby naming and clear the native deadline")]
        public async Task Name(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BabyNameRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Name(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Post("/api/v1/pawn/baby-name/delay")]
        [EndpointMetadata("Postpone the baby naming deadline (1-3 days)")]
        public async Task Delay(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BabyNameDelayRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Delay(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
