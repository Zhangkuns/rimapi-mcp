using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public sealed class PlantController
    {
        private readonly IGrowingService _growing;
        private readonly AiMutationCoordinator _mutations;

        public PlantController(IGrowingService growing, AiMutationCoordinator mutations)
        {
            _growing = growing;
            _mutations = mutations;
        }

        static int? OptionalInt(HttpListenerContext context, string name)
        {
            var value = context.Request.QueryString[name];
            if (string.IsNullOrEmpty(value)) return null;
            return int.TryParse(value, out var parsed) ? parsed : (int?)null;
        }

        [Get("/api/v1/plant/growing")]
        [EndpointMetadata("Read an existing growing zone or Building_PlantGrower: exact cells, crop policy, legal crops and plants")]
        public async Task Get(HttpListenerContext context)
        {
            var mapId = RequestParser.GetMapId(context);
            var zoneId = OptionalInt(context, "zone_id");
            var thingId = OptionalInt(context, "thing_id");
            await context.SendJsonResponse(_growing.Get(mapId, zoneId, thingId));
        }

        [Get("/api/v1/plant/plantable")]
        [EndpointMetadata("Read a native CompPlantable seed's selected cells and plant target")]
        public async Task GetPlantable(HttpListenerContext context)
        {
            var seedId = RequestParser.GetIntParameter(context, "seed_id");
            await context.SendJsonResponse(_growing.GetPlantable(
                new PlantableRequestDto { SeedId = seedId }));
        }

        [Post("/api/v1/plant/plantable")]
        [EndpointMetadata("Set/cancel a seed's native PlantCells or start a PlantSeed job")]
        public async Task ApplyPlantable(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PlantableRequestDto>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _growing.ApplyPlantable(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/plant/growing")]
        [EndpointMetadata("Manage an existing Zone_Growing or Building_PlantGrower: set_crop, set_policy, resize, rename or delete")]
        public async Task Apply(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GrowingRequestDto>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _growing.Apply(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }
    }
}
