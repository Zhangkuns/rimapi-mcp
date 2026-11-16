// AI-FORK: HTTP surface for Phase 1b control operations.
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class AiControlController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IAiControlService _controlService;

        public AiControlController(IAiControlService controlService, AiMutationCoordinator mutations)
        {
            _controlService = controlService;
            _mutations = mutations;
        }

        [Post("/api/v1/order/designate")]
        [EndpointMetadata("Designate any DesignationDef on cells or things (remove=true to clear)")]
        public async Task Designate(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DesignateRequestV2>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _controlService.Designate(body));
        }

        [Post("/api/v1/pawn/hediff")]
        [EndpointMetadata("Add a hediff to a pawn (hediff_def, body_part, severity, count)")]
        public async Task AddHediff(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<HediffRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _controlService.AddHediff(body));
        }

        [Delete("/api/v1/pawn/hediff")]
        [EndpointMetadata("Remove matching hediffs from a pawn (all=true removes every match)")]
        public async Task RemoveHediff(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<HediffRemoveRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _controlService.RemoveHediff(body));
        }

        [Post("/api/v1/pawn/hediff/severity")]
        [EndpointMetadata("Set severity on matching hediffs")]
        public async Task SetHediffSeverity(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<HediffSeverityRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _controlService.SetHediffSeverity(body));
        }

        [Post("/api/v1/pawn/mental-state")]
        [EndpointMetadata("Start a mental state (state_def, reason, force)")]
        public async Task StartMentalState(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MentalStateRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _controlService.StartMentalState(body));
        }

        [Delete("/api/v1/pawn/mental-state")]
        [EndpointMetadata("Recover a pawn from its current mental state")]
        public async Task StopMentalState(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnIdRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _controlService.StopMentalState(body));
        }

        [Post("/api/v1/pawn/inspiration")]
        [EndpointMetadata("Start an inspiration (inspiration_def, reason)")]
        public async Task StartInspiration(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<InspirationRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _controlService.StartInspiration(body));
        }

        [Post("/api/v1/pawn/order")]
        [EndpointMetadata("Validated order: goto / attack / attack_static (cell, thing_id or target_pawn_id)")]
        public async Task IssueOrder(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnOrderRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _controlService.IssueOrder(body));
        }
    }
}
