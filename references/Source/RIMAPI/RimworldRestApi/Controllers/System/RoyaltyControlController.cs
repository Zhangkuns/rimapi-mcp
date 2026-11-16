// AI-FORK: Royalty endpoints. Grant/remove touch only the ability tracker and go
// through the mutation coordinator; permits are read-only.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class RoyaltyControlController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IRoyaltyControlService _service;

        public RoyaltyControlController(
            IRoyaltyControlService service,
            AiMutationCoordinator mutations
        )
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/royalty/psycasts")]
        [EndpointMetadata("Read psylink level and known abilities for a colonist")]
        public async Task GetPsycasts(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetPsycasts(context.Request.QueryString["pawn_id"])
            );
        }

        [Get("/api/v1/dlc/royalty/permit/options")]
        [EndpointMetadata("Read Empire permit definitions and native availability")]
        public async Task GetPermitOptions(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetPermitOptions(
                    context.Request.QueryString["pawn_id"],
                    context.Request.QueryString["faction_id"])
            );
        }

        [Get("/api/v1/dlc/royalty/permits")]
        [EndpointMetadata("Read faction permits (read-only; never spends a permit)")]
        public async Task GetPermits(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetPermits(context.Request.QueryString["pawn_id"])
            );
        }

        [Get("/api/v1/dlc/royalty/persona")]
        [EndpointMetadata("Read persona-weapon eligibility, traits and native confirmation warning")]
        public async Task GetPersona(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetPersona(
                    context.Request.QueryString["pawn_id"],
                    context.Request.QueryString["thing_id"]
                )
            );
        }

        [Post("/api/v1/dlc/royalty/persona")]
        [EndpointMetadata("Equip a persona weapon after native CanEquip and confirmation")]
        public async Task EquipPersona(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RoyalPersonaEquipRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EquipPersona(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/royalty/monument")]
        [EndpointMetadata("Read spawned monument markers, allowed materials and blueprint state")]
        public async Task GetMonuments(HttpListenerContext context)
        {
            int mapId;
            if (!int.TryParse(context.Request.QueryString["map_id"], out mapId))
                mapId = 0;
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetMonuments(mapId)
            );
        }

        [Post("/api/v1/dlc/royalty/monument")]
        [EndpointMetadata("Place monument blueprints with a chosen material or cancel the marker")]
        public async Task ExecuteMonument(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RoyalMonumentRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ExecuteMonument(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/royalty/tribute")]
        [EndpointMetadata("Read royal-favor traders and native tribute candidates on a map")]
        public async Task GetTribute(HttpListenerContext context)
        {
            int mapId;
            if (!int.TryParse(context.Request.QueryString["map_id"], out mapId))
                mapId = 0;
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetTribute(
                    mapId,
                    context.Request.QueryString["negotiator_pawn_id"]
                )
            );
        }

        [Post("/api/v1/dlc/royalty/tribute")]
        [EndpointMetadata("Hand over gold tribute to a royal-favor trader and gain native favor")]
        public async Task ExecuteTribute(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RoyalTributeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ExecuteTribute(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/psycast")]
        [EndpointMetadata("Grant one whitelisted psycast to a colonist with enough psylink")]
        public async Task Grant(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PsycastRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Grant(body),
                Changed
            );
        }

        [Delete("/api/v1/dlc/royalty/psycast")]
        [EndpointMetadata("Remove one whitelisted psycast that this pawn owns")]
        public async Task Remove(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PsycastRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Remove(body),
                Changed
            );
        }

        [Post("/api/v1/dlc/royalty/psycast/cast")]
        [EndpointMetadata("Validate and queue a native cast job for a whitelisted psycast")]
        public async Task Cast(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PsycastCastRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Cast(body)
            );
        }

        [Get("/api/v1/dlc/royalty/meditation")]
        [EndpointMetadata("Read psyfocus target, meditation spot/focus and current assignment")]
        public async Task GetMeditation(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetMeditation(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/royalty/meditation/limiter")]
        [EndpointMetadata("Toggle the native psychic entropy limiter")]
        public async Task SetEntropyLimiter(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<EntropyLimiterRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetEntropyLimiter(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/royalty/meditation")]
        [EndpointMetadata("Set the native desired psyfocus target for a psycaster")]
        public async Task SetMeditation(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MeditationRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetMeditation(body),
                Changed
            );
        }

        private static bool Applied(ApiResult<Dictionary<string, object>> result)
        {
            object applied;
            return result.Data != null
                && result.Data.TryGetValue("applied", out applied)
                && applied is bool
                && (bool)applied;
        }

        private static bool Changed(ApiResult<Dictionary<string, object>> result)
        {
            object changed;
            return result.Data != null
                && result.Data.TryGetValue("changed", out changed)
                && changed is bool
                && (bool)changed;
        }
    }
}
