using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class GlobalMapController : BaseController
    {
        private readonly IGlobalMapService _globalMapService;
        private readonly ICachingService _cachingService;

        public GlobalMapController(IGlobalMapService globalMapService, ICachingService cachingService)
        {
            _globalMapService = globalMapService;
            _cachingService = cachingService;
        }

        [Get("/api/v1/world/caravans")]
        public async Task GetCaravans(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "caravans",
                () => Task.FromResult(_globalMapService.GetCaravans()),
                expiration: TimeSpan.FromSeconds(1),
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/world/caravan/forming")]
        [EndpointMetadata("Read the native forming caravan lord, transferables and blockers")]
        public async Task GetFormingCaravan(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            var result = _globalMapService.GetFormingCaravan(mapId);
            // map_id is a selector, not a row filter; do not run QueryFilter over
            // the dictionary response.
            await ResponseBuilder.SendApiResult(context.Response, result);
        }

        [Post("/api/v1/world/caravan/forming")]
        [EndpointMetadata("Form, cancel or reform a native caravan through Dialog_FormCaravan")]
        public async Task ControlFormingCaravan(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CaravanFormingRequest>();
            await context.SendJsonResponse(_globalMapService.ControlCaravanForming(body));
        }

        [Get("/api/v1/world/caravan/path")]
        public async Task GetCaravanPath(HttpListenerContext context)
        {
            var id = RequestParser.GetIntParameter(context, "id");
            await _cachingService.CacheAwareResponseAsync(
                context,
                "caravans",
                () => Task.FromResult(_globalMapService.GetCaravanPath(id)),
                expiration: TimeSpan.FromSeconds(1),
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/world/caravan/journey")]
        [EndpointMetadata("Read a native caravan route, target and arrival actions")]
        public async Task GetCaravanJourney(HttpListenerContext context)
        {
            var id = RequestParser.GetIntParameter(context, "caravan_id");
            int? target = null;
            var rawTarget = context.Request.QueryString["target_tile"];
            if (!string.IsNullOrWhiteSpace(rawTarget))
            {
                if (!int.TryParse(rawTarget, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "target_tile must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                target = parsed;
            }
            int? objectId = null;
            var rawObject = context.Request.QueryString["world_object_id"];
            if (!string.IsNullOrWhiteSpace(rawObject))
            {
                if (!int.TryParse(rawObject, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "world_object_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                objectId = parsed;
            }
            var result = _globalMapService.GetCaravanJourney(id, target, objectId);
            await ResponseBuilder.SendApiResult(context.Response, result);
        }

        [Post("/api/v1/world/caravan/journey")]
        [EndpointMetadata("Start a native caravan route, settle or abandon a player settlement")]
        public async Task ControlCaravanJourney(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CaravanJourneyRequest>();
            await context.SendJsonResponse(_globalMapService.ControlCaravanJourney(body));
        }

        [Get("/api/v1/letter/choices")]
        [EndpointMetadata("List native ChoiceLetter options and callbacks")]
        public async Task GetLetterChoices(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response,
                _globalMapService.GetLetterChoices());
        }

        [Post("/api/v1/letter/choices")]
        [EndpointMetadata("Invoke one explicit native ChoiceLetter option action")]
        public async Task ControlLetterChoice(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<LetterChoiceRequest>();
            await context.SendJsonResponse(_globalMapService.ControlLetterChoice(body));
        }

        [Get("/api/v1/utility/verbs")]
        [EndpointMetadata("List equipped apparel utility verbs, target types, charges and blockers")]
        public async Task GetUtilityVerbs(HttpListenerContext context)
        {
            int? pawnId = null;
            var raw = context.Request.QueryString["pawn_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "pawn_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                pawnId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response,
                _globalMapService.GetUtilityVerbs(pawnId));
        }

        [Post("/api/v1/utility/verbs")]
        [EndpointMetadata("Cast an equipped apparel utility verb at a pawn, thing or cell")]
        public async Task ControlUtilityVerb(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<UtilityVerbRequest>();
            await context.SendJsonResponse(_globalMapService.ControlUtilityVerb(body));
        }

        [Get("/api/v1/world/dialog")]
        [EndpointMetadata("Read open native business dialogs and their actionable options")]
        public async Task GetDialogDecision(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response,
                _globalMapService.GetDialogDecision(context.Request.QueryString["dialog_id"]));
        }

        [Post("/api/v1/world/dialog")]
        [EndpointMetadata("Invoke one explicit native business dialog option action")]
        public async Task ControlDialogDecision(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DialogDecisionRequest>();
            await context.SendJsonResponse(_globalMapService.ControlDialogDecision(body));
        }

        [Get("/api/v1/world/caravan/manage")]
        [EndpointMetadata("Read a player caravan's members, gear, inventory, path and policies")]
        public async Task GetCaravanManage(HttpListenerContext context)
        {
            var id = RequestParser.GetIntParameter(context, "caravan_id");
            await ResponseBuilder.SendApiResult(context.Response,
                _globalMapService.GetCaravanManage(id));
        }

        [Post("/api/v1/world/caravan/manage")]
        [EndpointMetadata("Split, merge, pause or manage a player caravan's members and gear")]
        public async Task ControlCaravanManage(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CaravanManageRequest>();
            await context.SendJsonResponse(_globalMapService.ControlCaravanManage(body));
        }

        [Get("/api/v1/transport/pod")]
        [EndpointMetadata("Read ordinary CompTransporter pods, holder contents and launch blockers")]
        public async Task GetTransportPods(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response,
                _globalMapService.GetTransportPods(mapId,
                    context.Request.QueryString["pod_id"]));
        }

        [Post("/api/v1/transport/pod")]
        [EndpointMetadata("Load, cancel loading or launch an ordinary transport pod")]
        public async Task ControlTransportPod(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TransportPodRequest>();
            await context.SendJsonResponse(_globalMapService.ControlTransportPod(body));
        }

        [Get("/api/v1/world/settlements")]
        public async Task GetSettlements(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "settlements",
                () => Task.FromResult(_globalMapService.GetSettlements()),
                expiration: TimeSpan.FromSeconds(1),
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/world/player/settlements")]
        public async Task GetPlayerSettlements(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "player_settlements",
                () => Task.FromResult(_globalMapService.GetPlayerSettlements()),
                expiration: TimeSpan.FromSeconds(1),
                expirationType: CacheExpirationType.Absolute
            );
        }


        [Get("/api/v1/world/sites")]
        public async Task GetSites(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "sites",
                () => Task.FromResult(_globalMapService.GetSites()),
                expiration: TimeSpan.FromSeconds(5),
                expirationType: CacheExpirationType.GameTick
            );
        }

        [Get("/api/v1/world/tile")]
        public async Task GetTile(HttpListenerContext context)
        {
            var tileId = RequestParser.GetIntParameter(context, "id");
            await _cachingService.CacheAwareResponseAsync(
                context,
                $"tile_{tileId}",
                () => Task.FromResult(_globalMapService.GetTile(tileId)),
                expiration: TimeSpan.FromSeconds(120),
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/world/tile/coordinates")]
        public async Task GetTileCoordinates(HttpListenerContext context)
        {
            var tileId = RequestParser.GetIntParameter(context, "id");
            await _cachingService.CacheAwareResponseAsync(
                context,
                $"tile_{tileId}_coordinates",
                () => Task.FromResult(_globalMapService.GetTileCoordinates(tileId)),
                expiration: TimeSpan.FromSeconds(120),
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/world/tile/details")]
        public async Task GetTileDetails(HttpListenerContext context)
        {
            var tileId = RequestParser.GetIntParameter(context, "id");
            await _cachingService.CacheAwareResponseAsync(
                context,
                $"tile_{tileId}_details",
                () => Task.FromResult(_globalMapService.GetTileDetails(tileId)),
                expiration: TimeSpan.FromSeconds(120),
                expirationType: CacheExpirationType.Absolute
            );
        }



        [Get("/api/v1/world/grid")]
        public async Task GetWorldGrid(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "world_grid",
                () => Task.FromResult(ApiResult<List<TileDto>>.Ok(GlobalMapHelper.GetWorldData())),
                expiration: TimeSpan.FromSeconds(600),
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/world/grid/area")]
        public async Task GetWorldGridArea(HttpListenerContext context)
        {
            if (!int.TryParse(context.Request.QueryString["tile_id"], out var tileId) ||
                !float.TryParse(context.Request.QueryString["radius"], out var radius))
            {
                await ResponseBuilder.SendError(context.Response, HttpStatusCode.BadRequest, "Invalid parameters. 'tile_id' (int) and 'radius' (float) are required.");
                return;
            }

            await _cachingService.CacheAwareResponseAsync(
                context,
                $"world_grid_area_{tileId}_{radius}",
                () => Task.FromResult(_globalMapService.GetTilesInRadius(tileId, radius)),
                expiration: TimeSpan.FromSeconds(600),
                expirationType: CacheExpirationType.Absolute
            );
        }
    }
}
