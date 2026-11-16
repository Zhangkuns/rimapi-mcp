using System.Collections.Generic;
// AI-FORK: trade inspection and execution.
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class TradeControlController
    {
        private readonly ITradeControlService _tradeControl;

        public TradeControlController(ITradeControlService tradeControl)
        {
            _tradeControl = tradeControl;
        }

        [Get("/api/v1/trade/traders")]
        [EndpointMetadata("List map traders and orbital ships, or traders at a player caravan's tile")]
        public async Task GetTraders(HttpListenerContext context)
        {
            var mapId = 0;
            int.TryParse(context.Request.QueryString["map_id"], out mapId);
            int? caravanId = null;
            if (context.Request.QueryString["caravan_id"] != null)
            {
                if (!int.TryParse(context.Request.QueryString["caravan_id"], out var parsed))
                {
                    await context.SendJsonResponse(ApiResult<object>.Fail("invalid caravan_id", ApiErrorCode.BadRequest));
                    return;
                }
                caravanId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _tradeControl.GetTraders(mapId, caravanId));
        }

        [Get("/api/v1/faction/comms")]
        [EndpointMetadata("Read faction comms targets, gates, cooldowns and AI-core prerequisites")]
        public async Task GetFactionComms(HttpListenerContext context)
        {
            var mapId = 0;
            int.TryParse(context.Request.QueryString["map_id"], out mapId);
            var pawnId = context.Request.QueryString["pawn_id"];
            var factionId = context.Request.QueryString["faction_id"];
            await ResponseBuilder.SendApiResult(context.Response,
                _tradeControl.GetFactionComms(mapId, pawnId, factionId));
        }

        [Post("/api/v1/faction/comms/request")]
        [EndpointMetadata("Open a real comms job or execute a native faction trader/aid/AI-core option")]
        public async Task RequestFactionComms(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FactionCommsRequest>();
            var result = _tradeControl.RequestFactionComms(body);
            var applied = result.Success && result.Data != null
                && result.Data.TryGetValue("applied", out var appliedValue)
                && appliedValue is bool didApply && didApply;
            AiEvent.PublishIfApplied(context, applied, body == null || body.DryRun);
            await ResponseBuilder.SendApiResult(context.Response, result);
        }

        [Get("/api/v1/trade/gift")]
        [EndpointMetadata("Read native gift-mode goods, recipient goodwill/relations and carry limit")]
        public async Task GetGift(HttpListenerContext context)
        {
            int.TryParse(context.Request.QueryString["map_id"], out var mapId);
            int.TryParse(context.Request.QueryString["trader_pawn_id"], out var traderId);
            var negotiatorId = context.Request.QueryString["negotiator_pawn_id"];
            await context.SendJsonResponse(_tradeControl.GetGift(mapId, traderId, negotiatorId));
        }

        [Post("/api/v1/trade/gift")]
        [EndpointMetadata("Quote or execute a native gift-mode TradeSession gift")]
        public async Task Gift(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TradeGiftRequest>();
            var action = (body?.Action ?? "").Trim().ToLowerInvariant();
            ApiResult<Dictionary<string, object>> result;
            if (action == "quote")
                result = _tradeControl.GiftQuote(body);
            else if (action == "execute")
            {
                result = _tradeControl.GiftExecute(body);
                var applied = result.Success && result.Data != null
                    && result.Data.TryGetValue("executed", out var executed)
                    && executed is bool didExecute && didExecute;
                if (applied && result.Data.TryGetValue("replayed", out var replayed)
                    && replayed is bool isReplay && isReplay) applied = false;
                AiEvent.PublishIfApplied(context, applied, body == null || body.DryRun);
            }
            else
                result = ApiResult<Dictionary<string, object>>.Fail(
                    "action must be quote or execute", ApiErrorCode.BadRequest);
            await ResponseBuilder.SendApiResult(context.Response, result);
        }

        [Post("/api/v1/trade/quote")]
        [EndpointMetadata("List native item groups or issue a 120-second quote for a map, orbital or caravan trade")]
        public async Task Quote(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TradeQuoteRequest>();
            await context.SendJsonResponse(_tradeControl.Quote(body));
        }

        [Post("/api/v1/trade/execute")]
        [EndpointMetadata("Execute a quote once with confirm and request_id; dry_run revalidates without settlement")]
        public async Task Execute(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TradeExecuteRequest>();
            var result = _tradeControl.Execute(body);
            // Publish before sending: a disconnected client must not suppress an applied event.
            // Replayed results and dry runs never emit another event.
            var applied = result.Success && result.Data != null
                && result.Data.TryGetValue("executed", out var executed) && executed is bool didExecute && didExecute
                && result.Data.TryGetValue("replayed", out var replayed) && replayed is bool isReplay && !isReplay;
            AiEvent.PublishIfApplied(context, applied, body == null || body.DryRun);
            await ResponseBuilder.SendApiResult(context.Response, result);
        }
    }
}
