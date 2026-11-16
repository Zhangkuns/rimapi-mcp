// AI-FORK: G-36 anomaly monolith read, G-37 containment read/release endpoints.
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class AnomalyControlController
    {
        private readonly IAnomalyControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public AnomalyControlController(
            IAnomalyControlService service,
            AiMutationCoordinator mutations
        )
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/anomaly/monolith")]
        [EndpointMetadata("Read monolith level, threat tier and activation state")]
        public async Task GetMonolith(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response, _service.GetMonolith());
        }

        [Post("/api/v1/dlc/anomaly/monolith/activate")]
        [EndpointMetadata("Activate the monolith once; irreversible level advance")]
        public async Task ActivateMonolith(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MonolithActivateRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ActivateMonolith(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/anomaly/monolith/void-structure/activate")]
        [EndpointMetadata("Order a colonist to activate a void structure or the gleaming monolith")]
        public async Task ActivateVoidStructure(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<VoidStructureActivateRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ActivateVoidStructure(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/anomaly/monolith/endgame")]
        [EndpointMetadata("Perform the void-node endgame choice: disrupt or embrace")]
        public async Task ChooseMonolithEndgame(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MonolithEndgameRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ChooseMonolithEndgame(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/anomaly/containment/cancel")]
        [EndpointMetadata("Cancel the pending native capture or transfer order for an exact entity")]
        public async Task CancelContainmentOrder(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ContainmentCancelRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.CancelContainmentOrder(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/anomaly/containment/release")]
        [EndpointMetadata("Release the entity contained on a holding platform")]
        public async Task ReleaseContained(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ContainmentReleaseRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReleaseEntity(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/anomaly/containment/capture")]
        [EndpointMetadata("Order a colonist to carry an exact capturable entity to an empty holding platform")]
        public async Task CaptureEntity(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ContainmentCaptureRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.CaptureEntity(body), Applied);
        }

        [Post("/api/v1/dlc/anomaly/containment/mode")]
        [EndpointMetadata("Set the native containment policy of an exact held entity")]
        public async Task SetContainmentMode(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ContainmentModeRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.SetContainmentMode(body), Applied);
        }

        [Post("/api/v1/dlc/anomaly/containment/work")]
        [EndpointMetadata("Order native release, study or execution work on an exact held entity")]
        public async Task OrderContainmentWork(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ContainmentWorkRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.OrderContainmentWork(body), Applied);
        }

        [Post("/api/v1/dlc/anomaly/containment/transfer")]
        [EndpointMetadata("Order native transfer of an exact held entity to another empty player platform")]
        public async Task TransferEntity(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ContainmentTransferRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.TransferEntity(body), Applied);
        }

        [Post("/api/v1/dlc/anomaly/prisoner/interrogate")]
        [EndpointMetadata("Queue the native warden identity-interrogation job")]
        public async Task InterrogatePrisoner(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<InterrogationRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.InterrogatePrisoner(body), Applied);
        }

        [Get("/api/v1/dlc/anomaly/death-refusal")]
        [EndpointMetadata("Read a pawn's native death-refusal self-resurrect state")]
        public async Task GetDeathRefusal(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetDeathRefusal(context.Request.QueryString["pawn_id"]));
        }

        [Post("/api/v1/dlc/anomaly/death-refusal/resurrect")]
        [EndpointMetadata("Use the dead pawn's native self-resurrect command")]
        public async Task SelfResurrect(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DeathRefusalRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SelfResurrect(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/anomaly/containment/extract-bioferrite")]
        [EndpointMetadata("Read the native ITab_Entity extract-bioferrite checkbox and eligibility")]
        public async Task GetExtractBioferrite(HttpListenerContext context)
        {
            int? mapId = null;
            int? platformId = null;
            var rawMap = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(rawMap))
            {
                if (!int.TryParse(rawMap, out var parsedMap))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsedMap;
            }
            var rawPlatform = context.Request.QueryString["platform_id"];
            if (!string.IsNullOrWhiteSpace(rawPlatform))
            {
                if (!int.TryParse(rawPlatform, out var parsedPlatform))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "platform_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                platformId = parsedPlatform;
            }
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetExtractBioferrite(mapId, platformId,
                    context.Request.QueryString["entity_id"]));
        }

        [Post("/api/v1/dlc/anomaly/containment/extract-bioferrite")]
        [EndpointMetadata("Set the held entity's native extract-bioferrite checkbox")]
        public async Task SetExtractBioferrite(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ExtractBioferriteRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetExtractBioferrite(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/anomaly/containment/bioferrite")]
        [EndpointMetadata("Toggle native bioferrite unloading or eject contained bioferrite")]
        public async Task SetBioferriteHarvester(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BioferriteHarvesterRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetBioferriteHarvester(body), Applied);
        }

        [Get("/api/v1/dlc/anomaly/knowledge")]
        [EndpointMetadata("Read anomaly knowledge categories, current project and legal candidates")]
        public async Task GetKnowledge(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetKnowledge(context.Request.QueryString["category"]));
        }

        [Post("/api/v1/dlc/anomaly/knowledge/stop")]
        [EndpointMetadata("Stop the active project of one native anomaly knowledge category")]
        public async Task StopKnowledge(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AnomalyKnowledgeStopRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.StopKnowledgeProject(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/anomaly/study")]
        [EndpointMetadata("Read the native CompStudiable state of an exact thing")]
        public async Task GetStudy(HttpListenerContext context)
        {
            int? mapId = null;
            int? thingId = null;
            var rawMap = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(rawMap))
            {
                if (!int.TryParse(rawMap, out var parsedMap))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsedMap;
            }
            var rawThing = context.Request.QueryString["thing_id"];
            if (string.IsNullOrWhiteSpace(rawThing) || !int.TryParse(rawThing, out var parsedThing))
            {
                await ResponseBuilder.SendApiResult(
                    context.Response,
                    ApiResult<Dictionary<string, object>>.Fail(
                        "thing_id must be an integer", ApiErrorCode.BadRequest));
                return;
            }
            thingId = parsedThing;
            await ResponseBuilder.SendApiResult(
                context.Response, _service.GetStudy(mapId, thingId));
        }

        [Post("/api/v1/dlc/anomaly/study")]
        [EndpointMetadata("Set the normal native study toggle of an exact studiable thing")]
        public async Task SetStudy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AnomalyStudyRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetStudy(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/anomaly/activity")]
        [EndpointMetadata("Read native activity and suppression policies for map entities")]
        public async Task GetActivity(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(context.Response, ApiResult<Dictionary<string, object>>.Fail("map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetActivity(mapId, context.Request.QueryString["target_id"]));
        }

        [Post("/api/v1/dlc/anomaly/activity/policy")]
        [EndpointMetadata("Set the native activity suppression checkbox and threshold for an exact target")]
        public async Task SetActivityPolicy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ActivityPolicyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.SetActivityPolicy(body), Applied);
        }

        [Post("/api/v1/dlc/anomaly/activity/suppress")]
        [EndpointMetadata("Prioritize native activity suppression of an exact target")]
        public async Task SuppressActivity(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ActivitySuppressRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.SuppressActivity(body), Applied);
        }

        [Get("/api/v1/dlc/anomaly/interactable")]
        [EndpointMetadata("Read native CompInteractable components of one thing (obelisk, golden cube, spine)")]
        public async Task GetInteractables(HttpListenerContext context)
        {
            int? mapId = null;
            int? thingId = null;
            var rawMap = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(rawMap))
            {
                if (!int.TryParse(rawMap, out var parsedMap))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsedMap;
            }
            var rawThing = context.Request.QueryString["thing_id"];
            if (string.IsNullOrWhiteSpace(rawThing) || !int.TryParse(rawThing, out var parsedThing))
            {
                await ResponseBuilder.SendApiResult(
                    context.Response,
                    ApiResult<Dictionary<string, object>>.Fail(
                        "thing_id must be an integer", ApiErrorCode.BadRequest));
                return;
            }
            thingId = parsedThing;
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetInteractables(mapId, thingId, context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/anomaly/interactable")]
        [EndpointMetadata("Order a colonist to use a native CompInteractable (native job and confirmation)")]
        public async Task Interact(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<InteractableRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Interact(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/anomaly/nociosphere")]
        [EndpointMetadata("Read a platform-held nociosphere send/activity state")]
        public async Task GetNociosphere(HttpListenerContext context)
        {
            int? mapId = null;
            int? thingId = null;
            var rawMap = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(rawMap))
            {
                if (!int.TryParse(rawMap, out var parsedMap))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsedMap;
            }
            var rawThing = context.Request.QueryString["thing_id"];
            if (string.IsNullOrWhiteSpace(rawThing) || !int.TryParse(rawThing, out var parsedThing))
            {
                await ResponseBuilder.SendApiResult(
                    context.Response,
                    ApiResult<Dictionary<string, object>>.Fail(
                        "thing_id must be an integer", ApiErrorCode.BadRequest));
                return;
            }
            thingId = parsedThing;
            await ResponseBuilder.SendApiResult(
                context.Response, _service.GetNociosphere(mapId, thingId));
        }

        [Post("/api/v1/dlc/anomaly/nociosphere/send")]
        [EndpointMetadata("Send the platform-held nociosphere to an exact destination cell")]
        public async Task SendNociosphere(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<NociosphereSendRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SendNociosphere(body),
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

        [Post("/api/v1/dlc/anomaly/mutation/transform")]
        [EndpointMetadata("Apply a MutantDef to a player-owned pawn")]
        public async Task TransformMutant(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MutantTransformRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.TransformMutant(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/anomaly/mutation/revert")]
        [EndpointMetadata("Revert a player-owned mutant to its original pawn")]
        public async Task RevertMutant(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MutantRevertRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.RevertMutant(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/anomaly/mutation/resurrect")]
        [EndpointMetadata("Resurrect an exact corpse as a native shambler")]
        public async Task ResurrectShambler(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MutantResurrectRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ResurrectShambler(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/anomaly/creepjoiner")]
        [EndpointMetadata("Read creep joiner offers, response eligibility and capture progress")]
        public async Task GetCreepJoiners(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(context.Response,
                        ApiResult<Dictionary<string, object>>.Fail("map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response,
                _service.GetCreepJoiners(mapId, context.Request.QueryString["pawn_id"]));
        }

        [Post("/api/v1/dlc/anomaly/creepjoiner/respond")]
        [EndpointMetadata("Accept, reject or capture a creep joiner via its native quest signals")]
        public async Task RespondCreepJoiner(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CreepJoinerRespondRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.RespondCreepJoiner(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/anomaly/mutation")]
        [EndpointMetadata("Read mutant state for the map or a single pawn")]
        public async Task GetMutation(HttpListenerContext context)
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
                _service.GetMutation(mapId, context.Request.QueryString["pawn_id"])
            );
        }

        [Get("/api/v1/dlc/anomaly/containment")]
        [EndpointMetadata("Read holding platforms, contained entities and escape state")]
        public async Task GetContainment(HttpListenerContext context)
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
                _service.GetContainment(mapId)
            );
        }
    }
}
