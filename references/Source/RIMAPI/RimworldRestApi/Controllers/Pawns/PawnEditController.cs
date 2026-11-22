using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PawnEditController
    {
        private readonly IPawnEditService _pawnEditService;
        private readonly AiMutationCoordinator _mutations;

        public PawnEditController(IPawnEditService pawnEditService,
            AiMutationCoordinator mutations)
        {
            _pawnEditService = pawnEditService;
            _mutations = mutations;
        }

        [Post("/api/v1/pawn/edit/basic")]
        public async Task EditBasicInfo(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnBasicRequest>();
            var result = _pawnEditService.UpdateBasicInfo(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/health")]
        public async Task EditHealth(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnHealthRequest>();
            var result = _pawnEditService.UpdateHealth(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/needs")]
        public async Task EditNeeds(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnNeedsRequest>();
            await _mutations.RunOnMain(() => context.SendJsonResponse(_pawnEditService.UpdateNeeds(body)));
        }

        [Post("/api/v1/pawn/edit/skills")]
        public async Task EditSkills(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnSkillsRequest>();
            var result = _pawnEditService.UpdateSkills(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/traits")]
        public async Task EditTraits(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnTraitsRequest>();
            var result = _pawnEditService.UpdateTraits(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/inventory")]
        public async Task EditInventory(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnInventoryRequest>();
            var result = _pawnEditService.UpdateInventory(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawn/equipment")]
        [EndpointMetadata("Read internal inventory, equipment, worn apparel (forced) and current job")]
        public async Task GetEquipment(HttpListenerContext context)
        {
            var pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _pawnEditService.GetEquipment(
                new PawnEquipmentRequest { PawnId = pawnId });
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/equipment")]
        [EndpointMetadata("Normal pickup, drop or dress-another-pawn action using native jobs")]
        public async Task ApplyEquipment(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnEquipmentRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _pawnEditService.ApplyEquipment(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/pawn/edit/apparel")]
        public async Task EditApparel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnApparelRequest>();
            var result = _pawnEditService.UpdateApparel(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/status")]
        public async Task EditStatus(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnStatusRequest>();
            var result = _pawnEditService.UpdateStatus(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/position")]
        public async Task EditPosition(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnPositionRequest>();
            var result = _pawnEditService.UpdatePosition(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/faction")]
        public async Task EditFaction(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnFactionRequest>();
            var result = _pawnEditService.UpdateFaction(body);
            await context.SendJsonResponse(result);
        }
        [Post("/api/v1/pawn/edit/recruitable")]
        [EndpointMetadata("WORK-P01: set one prisoner's native Recruitable flag and read back the effective value")]
        public async Task EditRecruitable(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnRecruitableRequest>();
            var result = _pawnEditService.SetRecruitable(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/trait/remove")]
        [EndpointMetadata("WORK-P01: remove one exact Trait(def, degree) and read back traits and genes")]
        public async Task RemoveTraitExact(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnTraitRemoveRequest>();
            var result = _pawnEditService.RemoveTraitExact(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/memory/remove")]
        [EndpointMetadata("WORK-P01: remove one selected mood or social memory")]
        public async Task RemoveMemory(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnMemoryRemoveRequest>();
            var result = _pawnEditService.RemoveMemory(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/inspiration/end")]
        [EndpointMetadata("WORK-P01: end the pawn's current inspiration")]
        public async Task EndInspiration(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnInspirationEndRequest>();
            var result = _pawnEditService.EndInspiration(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/backstory")]
        [EndpointMetadata("WORK-P01: replace the childhood or adulthood backstory and read back disabled work")]
        public async Task SetBackstory(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnBackstoryRequest>();
            var result = _pawnEditService.SetBackstory(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/prisoner-state")]
        [EndpointMetadata("WORK-P01: set a prisoner's resistance and/or will without recruiting")]
        public async Task SetPrisonerState(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnPrisonerStateRequest>();
            var result = _pawnEditService.SetPrisonerState(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/edit/item-attributes")]
        [EndpointMetadata("WORK-W02: set durability or quality on one exact carried item instance")]
        public async Task EditItemAttributes(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnItemAttributesRequest>();
            await _mutations.RunOnMain(() => context.SendJsonResponse(_pawnEditService.UpdateItemAttributes(body)));
        }

        [Post("/api/v1/pawn/memory/add")]
        [EndpointMetadata("WORK-W03: add one mood or social memory through native TryGainMemory")]
        public async Task AddMemory(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnMemoryAddRequest>();
            await _mutations.RunOnMain(() => context.SendJsonResponse(_pawnEditService.AddMemory(body)));
        }

        [Post("/api/v1/pawn/edit/skill-xp")]
        [EndpointMetadata("WORK-W04: set one skill's in-level experience without changing level or passion")]
        public async Task UpdateSkillXp(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnSkillXpRequest>();
            await _mutations.RunOnMain(() => context.SendJsonResponse(_pawnEditService.UpdateSkillXp(body)));
        }
    }
}
