using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class AssignmentController
    {
        private readonly IAssignmentService _service;
        private readonly AiMutationCoordinator _mutations;

        public AssignmentController(IAssignmentService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/building/assignable")]
        [EndpointMetadata("Read native assignable pawns on a bed/throne/spot")]
        public async Task Get(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.Get(context.Request.QueryString["thing_id"]));

        [Post("/api/v1/building/assignable")]
        [EndpointMetadata("Assign or unassign one pawn on an assignable building")]
        public async Task Assign(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AssignableRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Assign(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Get("/api/v1/building/bed")]
        [EndpointMetadata("Read bed owner type/medical settings")]
        public async Task GetBed(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.GetBed(context.Request.QueryString["thing_id"]));

        [Post("/api/v1/building/bed")]
        [EndpointMetadata("Set bed prisoner/medical usage")]
        public async Task SetBed(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BedSettingsRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetBed(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
