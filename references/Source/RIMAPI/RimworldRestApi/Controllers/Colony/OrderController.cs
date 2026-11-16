using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class OrderController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Post("/api/v1/order/designate/area")]
        public async Task DesignateArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<DesignateRequestDto>();
            var result = _orderService.DesignateArea(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/building/claim")]
        public async Task GetClaim(HttpListenerContext context)
        {
            var result = _orderService.ClaimThing(new ClaimThingRequestDto
            {
                ThingId = RequestParser.GetStringParameter(context, "thing_id"),
                DryRun = true,
            });
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/building/claim")]
        public async Task ClaimThing(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ClaimThingRequestDto>();
            var result = _orderService.ClaimThing(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawn/adopt")]
        public async Task GetAdopt(HttpListenerContext context)
        {
            var result = _orderService.AdoptPawn(new AdoptPawnRequestDto
            {
                PawnId = RequestParser.GetStringParameter(context, "pawn_id"),
                DryRun = true,
            });
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/pawn/adopt")]
        public async Task AdoptPawn(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AdoptPawnRequestDto>();
            var result = _orderService.AdoptPawn(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/map/mine-vein")]
        public async Task MineVein(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MineVeinRequestDto>();
            var result = _orderService.MineVein(body);
            await context.SendJsonResponse(result);
        }
    }
}