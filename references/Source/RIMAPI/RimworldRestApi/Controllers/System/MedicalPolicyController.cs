using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class MedicalPolicyController
    {
        private readonly IMedicalPolicyService _service;
        private readonly AiMutationCoordinator _mutations;

        public MedicalPolicyController(IMedicalPolicyService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/medical/defaults")]
        [EndpointMetadata("Read native medical-care defaults by pawn category")]
        public async Task GetDefaults(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.GetDefaults());

        [Post("/api/v1/medical/defaults")]
        [EndpointMetadata("Set native medical-care defaults by category")]
        public async Task SetDefaults(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MedicalDefaultsRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetDefaults(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Get("/api/v1/pawn/medical-settings")]
        [EndpointMetadata("Read a pawn's medical care and self-tend")]
        public async Task GetPawn(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.GetPawn(
                context.Request.QueryString["pawn_id"]));

        [Post("/api/v1/pawn/medical-settings")]
        [EndpointMetadata("Set a pawn's medical care/self-tend")]
        public async Task SetPawn(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnMedicalSettingsRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetPawn(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
