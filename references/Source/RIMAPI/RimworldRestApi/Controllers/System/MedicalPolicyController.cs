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

        [Get("/api/v1/pawn/medicine-carry")]
        [EndpointMetadata("Read the pawn's native Medicine inventory-stock type, target count and actual inventory")]
        public async Task GetMedicineCarry(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.GetMedicineCarry(context.Request.QueryString["pawn_id"]));

        [Post("/api/v1/pawn/medicine-carry")]
        [EndpointMetadata("Set native per-pawn Medicine carry type and target count without spawning or forcing pickup")]
        public async Task SetMedicineCarry(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MedicineCarryRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetMedicineCarry(body), r => r.Data != null && r.Data.ContainsKey("applied") && (bool)r.Data["applied"]);
        }

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
