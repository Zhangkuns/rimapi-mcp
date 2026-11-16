using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class RomanceController
    {
        private readonly IRomanceService _service;
        private readonly AiMutationCoordinator _mutations;

        public RomanceController(IRomanceService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/pawn/romance")]
        [EndpointMetadata("Read romance eligibility/opinion/chance between two pawns")]
        public async Task Get(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.Get(
                context.Request.QueryString["pawn_id"],
                context.Request.QueryString["target_id"]));

        [Post("/api/v1/pawn/romance")]
        [EndpointMetadata("Start a normal TryRomance job with RomanceAttempt interaction")]
        public async Task Attempt(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RomanceRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Attempt(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
