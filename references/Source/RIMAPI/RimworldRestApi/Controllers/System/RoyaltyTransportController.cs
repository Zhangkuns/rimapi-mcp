// AI-FORK: G-29 Task C restricted transport boarding endpoint.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class RoyaltyTransportController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IRoyaltyTransportService _service;

        public RoyaltyTransportController(
            IRoyaltyTransportService service,
            AiMutationCoordinator mutations
        )
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/odyssey/portal")]
        [EndpointMetadata("Read MapPortal load rows, candidates and loading lord")]
        public async Task GetPortal(HttpListenerContext context)
        {
            int? mapId = null;
            var rawMap = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(rawMap))
            {
                if (!int.TryParse(rawMap, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetPortal(context.Request.QueryString["portal_id"], mapId));
        }

        [Post("/api/v1/dlc/odyssey/portal")]
        [EndpointMetadata("Load, resize or cancel a MapPortal multi-pawn/item queue")]
        public async Task PortalAction(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<OdysseyPortalRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.PortalAction(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/odyssey/shuttle")]
        [EndpointMetadata("Read map/caravan passenger shuttles, cargo hold, fuel and launch blockers")]
        public async Task GetShuttle(HttpListenerContext context)
        {
            int? mapId = null;
            var rawMap = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(rawMap))
            {
                if (!int.TryParse(rawMap, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            int? caravanId = null;
            var rawCaravan = context.Request.QueryString["caravan_id"];
            if (!string.IsNullOrWhiteSpace(rawCaravan))
            {
                if (!int.TryParse(rawCaravan, out var parsedCaravan))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "caravan_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                caravanId = parsedCaravan;
            }
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetShuttle(
                    context.Request.QueryString["shuttle_id"], mapId, caravanId));
        }

        [Post("/api/v1/dlc/odyssey/shuttle")]
        [EndpointMetadata("Load, cancel, refuel from cargo or launch a map/caravan passenger shuttle")]
        public async Task ShuttleAction(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<OdysseyShuttleRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ShuttleAction(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/royalty/transport/ships")]
        [EndpointMetadata("Read every live transport ship, its current job and contained/required pawns")]
        public async Task GetShips(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response, _service.GetShips());
        }

        [Post("/api/v1/dlc/royalty/transport/ships/action")]
        [EndpointMetadata("Send a satisfied quest shuttle, dismiss a permit shuttle or set native automatic loading")]
        public async Task ShipAction(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TransportShipActionRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.ShipAction(body), Applied);
        }

        [Post("/api/v1/dlc/royalty/transport/board")]
        [EndpointMetadata("Queue the native EnterTransporter job for one pawn and transport ship")]
        public async Task Board(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TransportBoardRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Board(body),
                Queued
            );
        }

        [Post("/api/v1/dlc/royalty/transport/carry")]
        [EndpointMetadata("Queue the native HaulToTransporter job for one downed pawn and transport ship")]
        public async Task Carry(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TransportCarryRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Carry(body),
                Queued
            );
        }

        [Post("/api/v1/dlc/royalty/transport/carry/cancel")]
        [EndpointMetadata("Cancel one exact native HaulToTransporter carry job")]
        public async Task CancelCarry(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<CarryCancelRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.CancelCarry(body), Applied);
        }

        [Post("/api/v1/dlc/royalty/transport/loading/cancel")]
        [EndpointMetadata("Cancel one exact native transport loading group")]
        public async Task CancelLoading(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<LoadingCancelRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.CancelLoading(body), Applied);
        }

        [Post("/api/v1/dlc/royalty/transport/travel")]
        [EndpointMetadata("Submit one exact permit shuttle to a world destination through the native Launch action")]
        public async Task Travel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TransportTravelRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Travel(body), Applied);
        }

        private static bool Applied(ApiResult<Dictionary<string, object>> result)
        {
            object applied;
            return result.Data != null && result.Data.TryGetValue("applied", out applied)
                && applied is bool && (bool)applied;
        }

        private static bool Queued(ApiResult<Dictionary<string, object>> result)
        {
            object queued;
            return result.Data != null
                && result.Data.TryGetValue("queued", out queued)
                && queued is bool
                && (bool)queued;
        }
    }
}
