// AI-FORK: G-35 biotech life-state read endpoint.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class LifeControlController
    {
        private readonly ILifeControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public LifeControlController(ILifeControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/life/deathrest/carry")]
        [EndpointMetadata("Read a deathresting pawn's carry eligibility and candidate bed")]
        public async Task GetDeathrestCarry(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetDeathrestCarry(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/biotech/life/deathrest/carry")]
        [EndpointMetadata("Queue the native DeliverToBed job for a deathresting pawn")]
        public async Task CarryDeathrest(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DeathrestCarryRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.CarryDeathrest(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/subcore/eject")]
        [EndpointMetadata("Eject scanner contents; ripscanner ejection requires confirm")]
        public async Task EjectSubcore(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SubcoreEjectRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EjectSubcore(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/subcore")]
        [EndpointMetadata("Read subcore scanners, required ingredients and occupant state")]
        public async Task GetSubcore(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetSubcore(mapId));
        }

        [Post("/api/v1/dlc/biotech/subcore/init")]
        [EndpointMetadata("Initialize a powered subcore scanner through its native start gizmo")]
        public async Task InitSubcore(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SubcoreInitRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.InitSubcore(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/subcore/select")]
        [EndpointMetadata("Select a donor/prisoner for a scanner or cancel the load selection")]
        public async Task SelectSubcore(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SubcoreSelectRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SelectSubcore(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/biotech/subcore/load")]
        [EndpointMetadata("Order a pawn to haul one required ingredient into a scanner")]
        public async Task LoadSubcore(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SubcoreLoadRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.LoadSubcore(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/subcore/enter")]
        [EndpointMetadata("Order a pawn to enter an initialized, loaded subcore scanner")]
        public async Task EnterSubcore(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SubcoreEnterRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EnterSubcore(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/waste")]
        [EndpointMetadata("Read wastepack atomizers and wastepacks on a map")]
        public async Task GetWaste(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetWaste(mapId));
        }

        [Post("/api/v1/dlc/biotech/waste/atomizer/policy")]
        [EndpointMetadata("Set atomizer autoLoad or eject its native wastepack contents")]
        public async Task SetAtomizerPolicy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AtomizerPolicyRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetAtomizerPolicy(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/biotech/waste/atomize")]
        [EndpointMetadata("Order a pawn to haul wastepacks into a powered atomizer")]
        public async Task AtomizeWaste(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<WasteAtomizeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.AtomizeWaste(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/pollution")]
        [EndpointMetadata("Read map pollution cells, pollution-clear area and pumps/atomizers")]
        public async Task GetPollution(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetPollution(mapId));
        }

        [Get("/api/v1/dlc/biotech/growthvat")]
        [EndpointMetadata("Read growth vats, nutrition, selected pawn/embryo and contents")]
        public async Task GetGrowthVat(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer",
                            ApiErrorCode.BadRequest
                        )
                    );
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetGrowthVat(mapId));
        }

        [Post("/api/v1/dlc/biotech/growthvat/enter")]
        [EndpointMetadata("Order a standing colonist into a growth vat")]
        public async Task EnterGrowthVat(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GrowthVatEnterRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EnterGrowthVat(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/growthvat/embryo")]
        [EndpointMetadata("Select a spawned human embryo for a growth vat (native SelectEmbryo)")]
        public async Task SelectGrowthVatEmbryo(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GrowthVatEmbryoRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SelectGrowthVatEmbryo(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/growthvat/cancel")]
        [EndpointMetadata("Cancel a pending growth vat load before the vat starts working")]
        public async Task CancelGrowthVat(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GrowthVatCancelRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.CancelGrowthVatLoad(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/hemogen")]
        [EndpointMetadata("Read a pawn's hemogen gene, bloodfeeder state and feedable prisoners")]
        public async Task GetHemogen(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetHemogen(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/biotech/hemogen/feed")]
        [EndpointMetadata("Queue the native prisoner bloodfeed job for a bloodfeeder")]
        public async Task FeedHemogen(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<HemogenFeedRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.FeedHemogen(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/life/hemogen/policy")]
        [EndpointMetadata("Set the native hemogen auto-consume threshold and packs policy")]
        public async Task SetHemogenPolicy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<HemogenPolicyRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetHemogenPolicy(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/biotech/life/deathrest/auto-wake")]
        [EndpointMetadata("Set the native Gene_Deathrest autoWake policy")]
        public async Task SetDeathrestAutoWake(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DeathrestAutoWakeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetDeathrestAutoWake(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/biotech/life/deathrest/wake")]
        [EndpointMetadata("Wake a deathresting pawn (native Gene_Deathrest.Wake)")]
        public async Task WakeDeathrest(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DeathrestWakeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.WakeDeathrest(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/life/deathrest/start")]
        [EndpointMetadata(
            "Order a pawn into deathrest in a bed (native Deathrest job, order only)"
        )]
        public async Task StartDeathrest(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DeathrestStartRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.StartDeathrest(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/growthmoment")]
        [EndpointMetadata("Read pending growth-moment letters and their native choices")]
        public async Task GetGrowthMoments(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response, _service.GetGrowthMoments());
        }

        [Post("/api/v1/dlc/biotech/growthmoment/choose")]
        [EndpointMetadata("Choose available native growth-moment passions/trait and complete the letter")]
        public async Task ChooseGrowthMoment(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GrowthMomentChoiceRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ChooseGrowthMoment(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/pregnancy-approach")]
        [EndpointMetadata("Read pregnancy approaches for a pawn's lovers/spouses")]
        public async Task GetPregnancyApproaches(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetPregnancyApproaches(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/biotech/pregnancy-approach")]
        [EndpointMetadata("Set a native pregnancy approach for one partner")]
        public async Task SetPregnancyApproach(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PregnancyApproachRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetPregnancyApproach(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/childcare")]
        [EndpointMetadata("Read a pawn's childcare needs and carried state")]
        public async Task GetChildcare(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetChildcare(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/biotech/childcare/autofeed")]
        [EndpointMetadata("Choose a native BabyFood and queue the autofeed job")]
        public async Task AutofeedBaby(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BabyAutofeedRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.AutofeedBaby(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/childcare/carry")]
        [EndpointMetadata("Queue the native BringBabyToSafety job for one baby")]
        public async Task CarryBaby(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BabyCarryRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.CarryBaby(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/childcare/breastfeed")]
        [EndpointMetadata("Queue the native breastfeed job for a hungry baby")]
        public async Task Breastfeed(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BreastfeedRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Breastfeed(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/childcare/feed")]
        [EndpointMetadata("Queue the native bottle-feed job for one hungry baby")]
        public async Task FeedBaby(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BabyFeedRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.FeedBaby(body),
                Applied
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

        [Get("/api/v1/dlc/biotech/life")]
        [EndpointMetadata("Read pregnancy and deathrest state for a pawn")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.Get(context.Request.QueryString["pawn_id"])
            );
        }
    }
}
