// AI-FORK: G-29b drop-resource permit endpoint (preview + native delivery).
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class RoyaltyPermitController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IRoyaltyPermitService _service;
        private readonly IRoyaltyPermitExecutionService _execution;

        public RoyaltyPermitController(
            IRoyaltyPermitService service,
            IRoyaltyPermitExecutionService execution,
            AiMutationCoordinator mutations
        )
        {
            _service = service;
            _execution = execution;
            _mutations = mutations;
        }

        [Post("/api/v1/dlc/royalty/permit/use")]
        [EndpointMetadata("Preview or execute a drop-resource permit (SteelDrop/FoodDrop)")]
        public async Task Use(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PermitUseRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Use(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/permit/purchase")]
        [EndpointMetadata("Buy a royal permit through the native AddPermit path")]
        public async Task Purchase(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PermitPurchaseRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.PurchasePermit(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/permit/refund")]
        [EndpointMetadata("Return every permit of a royal faction through the native RefundPermits path")]
        public async Task Refund(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PermitRefundRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.RefundPermits(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/permit/execute")]
        [EndpointMetadata("Execute a targeted royal-aid permit worker (aid/laborers/shuttle/strike)")]
        public async Task Execute(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PermitExecuteRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _execution.Execute(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/royalty/permit/caravan")]
        [EndpointMetadata("Read royal world-map permits held by a pawn travelling in a caravan")]
        public async Task CaravanStatus(HttpListenerContext context)
        {
            int targetTile;
            int? parsed = int.TryParse(
                context.Request.QueryString["target_tile"], out targetTile)
                ? targetTile
                : (int?)null;
            await ResponseBuilder.SendApiResult(
                context.Response,
                _execution.CaravanStatus(
                    context.Request.QueryString["pawn_id"],
                    parsed
                )
            );
        }

        [Post("/api/v1/dlc/royalty/permit/caravan")]
        [EndpointMetadata("Use SteelDrop/FoodDrop or CallTransportShuttle from a native caravan")]
        public async Task CaravanUse(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RoyalCaravanPermitRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _execution.ExecuteCaravan(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/royalty/permit/execute/status")]
        [EndpointMetadata("Read permit cooldown/favor and the worker's currently spawned objects")]
        public async Task ExecuteStatus(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                string.IsNullOrEmpty(context.Request.QueryString["operation_id"])
                    ? _execution.Status(
                        context.Request.QueryString["pawn_id"],
                        context.Request.QueryString["permit_def"]
                    )
                    : _execution.StatusByOperation(context.Request.QueryString["operation_id"])
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
    }
}
