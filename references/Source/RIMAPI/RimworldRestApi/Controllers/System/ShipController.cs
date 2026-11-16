using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class ShipController
    {
        private readonly IShipControlService _ship;
        public ShipController(IShipControlService ship) { _ship = ship; }

        [Get("/api/v1/ship/report")]
        [EndpointMetadata("Native ship parts, startup state, launch report")]
        public async Task Report(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _ship.Report(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/ship/startup")]
        [EndpointMetadata("Start native hibernating ship parts")]
        public async Task Startup(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ShipRequest>();
            await context.SendJsonResponse(_ship.Startup(body));
        }

        [Post("/api/v1/ship/launch")]
        [EndpointMetadata("Launch the ship through native ShipCountdown")]
        public async Task Launch(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ShipRequest>();
            await context.SendJsonResponse(_ship.Launch(body));
        }

        [Post("/api/v1/ship/cancel")]
        [EndpointMetadata("Cancel a running native ship countdown")]
        public async Task Cancel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ShipRequest>();
            await context.SendJsonResponse(_ship.CancelLaunch(body));
        }
    }
}
