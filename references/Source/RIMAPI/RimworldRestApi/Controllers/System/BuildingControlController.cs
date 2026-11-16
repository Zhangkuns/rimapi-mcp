using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class BuildingControlController
    {
        private readonly ITemperatureControlService _temperature;
        private readonly IFuelControlService _fuel;
        private readonly ITurretControlService _turret;

        public BuildingControlController(
            ITemperatureControlService temperature, IFuelControlService fuel,
            ITurretControlService turret)
        {
            _temperature = temperature;
            _fuel = fuel;
            _turret = turret;
        }

        [Get("/api/v1/building/temperature")]
        [EndpointMetadata("Read native temperature-control target and actual temperature")]
        public async Task GetTemperature(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _temperature.GetTemperature(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/building/temperature")]
        [EndpointMetadata("Set or raise/lower/reset a native temperature-control target")]
        public async Task SetTemperature(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TemperatureControlRequest>();
            await context.SendJsonResponse(_temperature.SetTemperature(body));
        }

        [Get("/api/v1/building/fuel")]
        [EndpointMetadata("Read native CompRefuelable target and auto-refuel policy")]
        public async Task GetFuel(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _fuel.GetFuel(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/building/fuel")]
        [EndpointMetadata("Set a native refuelable target and/or auto-refuel toggle")]
        public async Task SetFuel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FuelControlRequest>();
            await context.SendJsonResponse(_fuel.SetFuel(body));
        }

        [Post("/api/v1/building/fuel/refuel")]
        [EndpointMetadata("Order a normal native refuel job for a pawn")]
        public async Task OrderRefuel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FuelRefuelRequest>();
            await context.SendJsonResponse(_fuel.OrderRefuel(body));
        }

        [Get("/api/v1/building/turret")]
        [EndpointMetadata("Read turret forcing/hold-fire/shell/rocketswarm state")]
        public async Task GetTurret(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _turret.GetTurret(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/building/turret")]
        [EndpointMetadata("Force/clear turret target, toggle hold fire, manage shells or fire rocketswarm")]
        public async Task ControlTurret(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TurretControlRequest>();
            await context.SendJsonResponse(_turret.ControlTurret(body));
        }

        [Get("/api/v1/building/explosive")]
        [EndpointMetadata("Read CompExplosive wick and blast state")]
        public async Task GetExplosive(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _turret.GetExplosive(context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/building/explosive")]
        [EndpointMetadata("Start the native CompExplosive wick (no DEV instant blast)")]
        public async Task DetonateExplosive(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ExplosiveControlRequest>();
            await context.SendJsonResponse(_turret.DetonateExplosive(body));
        }
    }
}
