using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;
namespace RIMAPI.Controllers
{
    public class TakeToBedController
    {
        private readonly ITakeToBedService _service;
        private readonly AiMutationCoordinator _mutations;
        public TakeToBedController(ITakeToBedService service, AiMutationCoordinator mutations)
        { _service = service; _mutations = mutations; }
        [Get("/api/v1/pawn/take-to-bed")]
        [EndpointMetadata("Read current TakeToBed job or exact patient terminal state")]
        public async Task Get(HttpListenerContext context)
        {
            // The query keys are exact object selectors, not response-field
            // filters; bypass generic QueryFilter so the readback has no warning.
            var result = _service.Get(context.Request.QueryString["actor_id"],
                context.Request.QueryString["patient_id"]);
            await ResponseBuilder.SendApiResult(context.Response, result);
        }
        [Post("/api/v1/pawn/take-to-bed")]
        [EndpointMetadata("Submit native Rescue/Capture/Arrest/Escort TakeToBed job or release a carried patient")]
        public async Task Submit(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TakeToBedRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Submit(body));
        }
    }
}
