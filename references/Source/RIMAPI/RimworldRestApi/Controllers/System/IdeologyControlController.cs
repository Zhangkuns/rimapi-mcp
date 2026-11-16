// AI-FORK: G-30 ideology read and restricted conversion.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class IdeologyControlController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IIdeologyControlService _service;

        public IdeologyControlController(
            IIdeologyControlService service,
            AiMutationCoordinator mutations
        )
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/ideology")]
        [EndpointMetadata("Read the pawn's ideo, certainty and all ideos in this world")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.Get(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/ideology")]
        [EndpointMetadata("Convert a colonist to an existing ideo")]
        public async Task Convert(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoConvertRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Convert(body),
                Changed
            );
        }

        [Get("/api/v1/dlc/ideology/relic")]
        [EndpointMetadata("Read relic precepts, map relics and reliquaries")]
        public async Task GetRelic(HttpListenerContext context)
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
            await ResponseBuilder.SendApiResult(context.Response, _service.GetRelic(mapId));
        }

        [Post("/api/v1/dlc/ideology/relic/install")]
        [EndpointMetadata("Order a colonist to install an exact relic into an empty reliquary")]
        public async Task InstallRelic(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RelicInstallRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.InstallRelic(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/relic/extract")]
        [EndpointMetadata("Order a colonist to extract a relic from a reliquary")]
        public async Task ExtractRelic(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RelicExtractRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ExtractRelic(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/convert/ability")]
        [EndpointMetadata("Read a pawn's native Convert abilities and cooldown")]
        public async Task GetConvertAbility(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetConvertAbility(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/ideology/convert/ability")]
        [EndpointMetadata("Queue the native Convert ability against an exact target")]
        public async Task ConvertWithAbility(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoConvertAbilityRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ConvertWithAbility(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/prisoner/convert")]
        [EndpointMetadata("Set a colony prisoner's native Convert interaction mode and target ideo")]
        public async Task SetPrisonerConvert(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PrisonerConvertRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetPrisonerConvert(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/precepts")]
        [EndpointMetadata("List running ideo precept instances with stable IDs and options")]
        public async Task GetPrecepts(HttpListenerContext context)
        {
            var query = context.Request.QueryString;
            var includeAddOptions =
                query["include_add_options"] == "true" || query["include_add_options"] == "1";
            var includeStyle =
                query["include_style"] == "true" || query["include_style"] == "1";
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetPrecepts(query["ideo_id"], includeAddOptions, includeStyle));
        }

        [Get("/api/v1/dlc/ideology/precept/edit-options")]
        [EndpointMetadata("Read native-editable fields and legal values for one precept")]
        public async Task GetPreceptEditOptions(HttpListenerContext context)
        {
            int? preceptId = null;
            var raw = context.Request.QueryString["precept_id"];
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "precept_id must be a number",
                            ApiErrorCode.BadRequest));
                    return;
                }
                preceptId = parsed;
            }
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetPreceptEditOptions(
                    preceptId,
                    context.Request.QueryString["ideo_id"]));
        }

        [Post("/api/v1/dlc/ideology/precept/edit")]
        [EndpointMetadata("Edit name/lock, leader titles, role apparel, relic stuff, building style or ritual seat")]
        public async Task EditPrecept(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoPreceptEditRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EditPrecept(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/development")]
        [EndpointMetadata("Read the pawn's fluid ideo development/reform state")]
        public async Task GetDevelopment(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetDevelopment(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/ideology/reform/style")]
        [EndpointMetadata("Reform a fluid ideo by adding or removing one style category")]
        public async Task ReformStyle(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformStyleRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformStyle(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/precept")]
        [EndpointMetadata("Reform a fluid ideo by removing one removable precept")]
        public async Task ReformPrecept(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformPreceptRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformPrecept(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/role/assign")]
        [EndpointMetadata("Assign or clear one native Ideology role precept for a member pawn")]
        public async Task AssignRole(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoRoleAssignRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.AssignRole(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/tree")]
        [EndpointMetadata("Read one Gauranlen tree connection, dryad caste and pruning policy")]
        public async Task GetTree(HttpListenerContext context)
        {
            int? thingId = null;
            var raw = context.Request.QueryString["thing_id"];
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "thing_id must be a numeric thing id",
                            ApiErrorCode.BadRequest));
                    return;
                }
                thingId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetTree(thingId));
        }

        [Post("/api/v1/dlc/ideology/tree/policy")]
        [EndpointMetadata("Set the native desired connection strength or dryad caste on a connected Gauranlen tree")]
        public async Task SetTreePolicy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoTreePolicyRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetTreePolicy(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/tree/dryad")]
        [EndpointMetadata("Toggle the native heal-in-healing-pod flag on one connected dryad")]
        public async Task SetDryadReturn(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoTreeDryadRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetDryadReturn(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/slavery")]
        [EndpointMetadata("Read player prisoners and slaves with will/resistance/suppression")]
        public async Task GetSlavery(HttpListenerContext context)
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
                            "map_id must be a number",
                            ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(
                context.Response, _service.GetSlavery(mapId));
        }

        [Post("/api/v1/dlc/ideology/slavery/enslave")]
        [EndpointMetadata("Order the native PrisonerEnslave warden job")]
        public async Task EnslavePrisoner(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoSlaveryEnslaveRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EnslavePrisoner(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/slavery/emancipate")]
        [EndpointMetadata("Emancipate a player slave via GenGuest.EmancipateSlave")]
        public async Task EmancipateSlave(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoSlaveryEmancipateRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.EmancipateSlave(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/slavery/imprison")]
        [EndpointMetadata("Turn a slave into a prisoner through the native Arrest job")]
        public async Task ImprisonSlave(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoSlaveryImprisonRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.ImprisonSlave(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/slavery/suppress")]
        [EndpointMetadata("Queue the native SlaveSuppress warden job")]
        public async Task SuppressSlave(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoSlaverySuppressRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SuppressSlave(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/prisoner/reduce-will")]
        [EndpointMetadata("Queue the native PrisonerReduceWill warden job")]
        public async Task ReduceWill(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReduceWillRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.ReduceWill(body), Applied);
        }

        [Get("/api/v1/dlc/ideology/supercharger")]
        [EndpointMetadata("Read the player neural superchargers and their charged state")]
        public async Task GetSuperchargers(HttpListenerContext context)
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
                            "map_id must be a number",
                            ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(
                context.Response, _service.GetSuperchargers(mapId));
        }

        [Post("/api/v1/dlc/ideology/supercharger/policy")]
        [EndpointMetadata("Set the native neural supercharger auto-use mode and guest policy")]
        public async Task SetSuperchargerPolicy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SuperchargerPolicyRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetSuperchargerPolicy(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/ideology/supercharger/use")]
        [EndpointMetadata("Order the native neural supercharge job on a charged building")]
        public async Task UseSupercharger(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<NeuralSuperchargerUseRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.UseSupercharger(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/ritual/config")]
        [EndpointMetadata("Read the player-editable fields of one native ritual instance")]
        public async Task GetRitualConfig(HttpListenerContext context)
        {
            int? ritualId = null;
            var raw = context.Request.QueryString["ritual_id"];
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "ritual_id must be a number",
                            ApiErrorCode.BadRequest));
                    return;
                }
                ritualId = parsed;
            }
            await ResponseBuilder.SendApiResult(
                context.Response, _service.GetRitualConfig(ritualId));
        }

        [Post("/api/v1/dlc/ideology/ritual/config")]
        [EndpointMetadata("Rename a ritual or change its anytime/reward/date configuration")]
        public async Task SetRitualConfig(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoRitualConfigRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetRitualConfig(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/styling")]
        [EndpointMetadata("Read a pawn's native hair, beard, tattoo, color and styling options")]
        public async Task GetStyling(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetStyling(context.Request.QueryString["pawn_id"]));
        }

        [Post("/api/v1/dlc/ideology/styling")]
        [EndpointMetadata("Apply one native styling look change through the styling station job")]
        public async Task ApplyStyling(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoStylingRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ApplyStyling(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/precept/add")]
        [EndpointMetadata("Reform a fluid ideo by adding one plain precept")]
        public async Task ReformPreceptAdd(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformPreceptAddRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformPreceptAdd(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/precept/replace")]
        [EndpointMetadata("Reform a fluid ideo by replacing one plain precept in the same issue")]
        public async Task ReformPreceptReplace(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformPreceptReplaceRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformPreceptReplace(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/meme")]
        [EndpointMetadata("Reform a fluid ideo by adding or removing one normal meme")]
        public async Task ReformMeme(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformMemeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformMeme(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/combined")]
        [EndpointMetadata(
            "Apply one native combined fluid-ideo reform draft: one choose-one change "
                + "plus narrative lock, identity, appearance and plain precept edits"
        )]
        public async Task ReformCombined(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformCombinedRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformCombined(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/structure-meme")]
        [EndpointMetadata("Reform a fluid ideo by replacing its structure meme")]
        public async Task ReformStructureMeme(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformStructureMemeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformStructureMeme(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/identity")]
        [EndpointMetadata("Reform a fluid ideo name, adjective, member name or description")]
        public async Task ReformIdentity(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformIdentityRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformIdentity(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/symbol")]
        [EndpointMetadata("Reform a fluid ideo icon and/or color")]
        public async Task ReformSymbol(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformSymbolRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformSymbol(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/reform/deity")]
        [EndpointMetadata("Reform one deity name, type or gender on a fluid ideo")]
        public async Task ReformDeity(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoReformDeityRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReformDeity(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/presets")]
        [EndpointMetadata("List native in-game ideo preset files")]
        public async Task GetIdeoPresets(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetIdeoPresets()
            );
        }

        [Post("/api/v1/dlc/ideology/presets/save")]
        [EndpointMetadata("Save the running ideo as a native preset file")]
        public async Task SaveIdeoPreset(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<IdeoPresetSaveRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SaveIdeoPreset(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/ideology/biosculpter")]
        [EndpointMetadata("Read biosculpter pods, state, cycles and per-pawn eligibility")]
        public async Task GetBiosculpter(HttpListenerContext context)
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
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetBiosculpter(
                    mapId,
                    context.Request.QueryString["pawn_id"],
                    context.Request.QueryString["carrier_pawn_id"]
                )
            );
        }

        [Post("/api/v1/dlc/ideology/biosculpter/enter")]
        [EndpointMetadata("Order a pawn to enter a biosculpter pod with an exact cycle")]
        public async Task EnterBiosculpter(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BiosculpterEnterRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EnterBiosculpter(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/ideology/biosculpter/policy")]
        [EndpointMetadata("Set biosculpter auto-load-nutrition and auto-age-reversal")]
        public async Task SetBiosculpterPolicy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BiosculpterPolicyRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetBiosculpterPolicy(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/ideology/biosculpter/eject")]
        [EndpointMetadata("Eject the occupant of a biosculpter pod")]
        public async Task EjectBiosculpter(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BiosculpterEjectRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EjectBiosculpter(body),
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
