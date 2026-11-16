using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PawnJobController
    {
        private readonly IPawnJobService _pawnJobService;

        public PawnJobController(IPawnJobService pawnJobService)
        {
            _pawnJobService = pawnJobService;
        }

        [Post("/api/v1/pawn/job")]
        [EndpointMetadata("Assign a job to a pawn by JobDef name, with optional target")]
        public async Task AssignJob(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnJobRequestDto>();
            var result = _pawnJobService.AssignJob(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawn/combat-policy")]
        [EndpointMetadata("Read drafted combat policy, fire-at-will, hostility response and attack queue")]
        public async Task GetCombatPolicy(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _pawnJobService.GetCombatPolicy(
                new CombatPolicyRequestDto { PawnId = pawnId });
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawn/work-queue")]
        [EndpointMetadata("Read manual priorities, native priority work, current/queued jobs and fertilize state")]
        public async Task GetWorkQueue(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _pawnJobService.GetWorkQueue(new WorkQueueRequestDto { PawnId = pawnId });
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawn/reload")]
        [EndpointMetadata("Read equipped/worn native reloadable gear, charges, ammo and current job")]
        public async Task GetReload(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _pawnJobService.GetReload(
                new PawnReloadRequest { PawnId = pawnId });
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/reload")]
        [EndpointMetadata("Start a native Reload job for exact reloadable gear and its ammo queue")]
        public async Task ApplyReload(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnReloadRequest>();
            var result = _pawnJobService.ApplyReload(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/work-queue")]
        [EndpointMetadata("Run a named native work action: clean_room, extinguish_fires, prioritize, cancel_fertilize, fertilize or set_manual_priorities")]
        public async Task RunWorkQueueAction(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<WorkQueueRequestDto>();
            var result = _pawnJobService.RunWorkQueueAction(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/combat-policy")]
        [EndpointMetadata("Apply drafted/fire-at-will/hostility policy or stop the current native attack")]
        public async Task SetCombatPolicy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CombatPolicyRequestDto>();
            var result = _pawnJobService.SetCombatPolicy(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/medical/tend")]
        [EndpointMetadata("Assign a doctor to tend a patient")]
        public async Task AssignTendJob(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MedicalTendRequestDto>();
            var result = _pawnJobService.AssignTendJob(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/medical/bed-rest")]
        [EndpointMetadata("Assign a pawn to bed rest, optionally specifying a bed")]
        public async Task AssignBedRest(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MedicalBedRestRequestDto>();
            var result = _pawnJobService.AssignBedRest(body);
            await context.SendJsonResponse(result);
        }
    }
}
