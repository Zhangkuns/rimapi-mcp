// AI-FORK: G-40 Odyssey gravship read endpoint.
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Services;
using RIMAPI.Models;

namespace RIMAPI.Controllers
{
    public class GravshipControlController
    {
        private readonly IGravshipControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public GravshipControlController(IGravshipControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/odyssey/animal/training")]
        [EndpointMetadata("Read special animal trainables and their wanted/learned state")]
        public async Task GetAnimalTraining(HttpListenerContext context)
        {
            var animalId = context.Request.QueryString["animal_id"];
            await ResponseBuilder.SendApiResult(
                context.Response, _service.GetAnimalTraining(animalId));
        }

        [Post("/api/v1/dlc/odyssey/animal/training")]
        [EndpointMetadata("Set an animal training checkbox and optionally order a native Train job")]
        public async Task SetAnimalTraining(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AnimalTrainingRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetAnimalTraining(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/odyssey/environment")]
        [EndpointMetadata("Read exact cell/room/pawn vacuum, roof and room state")]
        public async Task GetEnvironment(HttpListenerContext context)
        {
            int? mapId = null;
            int? x = null;
            int? z = null;
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
            var rawX = context.Request.QueryString["x"];
            var rawZ = context.Request.QueryString["z"];
            if (!string.IsNullOrWhiteSpace(rawX) || !string.IsNullOrWhiteSpace(rawZ))
            {
                if (!int.TryParse(rawX, out var parsedX) || !int.TryParse(rawZ, out var parsedZ))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "x and z must both be integers", ApiErrorCode.BadRequest));
                    return;
                }
                x = parsedX;
                z = parsedZ;
            }
            await ResponseBuilder.SendApiResult(
                context.Response, _service.GetEnvironment(mapId, x, z));
        }

        [Get("/api/v1/dlc/odyssey/gravship")]
        [EndpointMetadata("Read gravships in the world, map engines and travel state")]
        public async Task Get(HttpListenerContext context)
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
            await ResponseBuilder.SendApiResult(context.Response, _service.Get(mapId, context.Request.QueryString["run_id"]));
        }

        static bool Applied(ApiResult<Dictionary<string, object>> result) =>
            result.Data != null && result.Data.TryGetValue("applied", out var value) && value is bool applied && applied;

        [Post("/api/v1/dlc/odyssey/gravship/refuel")]
        [EndpointMetadata("Order normal prioritized refueling of a connected gravship fuel component")]
        public async Task Refuel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GravshipMaintenanceRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Refuel(body), Applied);
        }

        [Post("/api/v1/dlc/odyssey/gravship/repair")]
        [EndpointMetadata("Order normal prioritized breakdown repair of a gravship component")]
        public async Task Repair(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GravshipMaintenanceRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Repair(body), Applied);
        }

        [Post("/api/v1/dlc/odyssey/gravship/launch/start")]
        [EndpointMetadata("Start normal native boarding with an exact pilot and crew")]
        public async Task Start(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GravshipLaunchRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Start(body), Applied);
        }

        [Post("/api/v1/dlc/odyssey/gravship/takeoff")]
        [EndpointMetadata("Choose a validated destination for a prepared gravship run and begin native takeoff")]
        public async Task Takeoff(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GravshipTakeoffRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Takeoff(body), Applied);
        }

        [Post("/api/v1/dlc/odyssey/gravship/launch/cancel")]
        [EndpointMetadata("Cancel the exact active native boarding ritual")]
        public async Task Cancel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GravshipCancelRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Cancel(body), Applied);
        }

        [Post("/api/v1/dlc/odyssey/gravship/land")]
        [EndpointMetadata("Validate and confirm an exact arrived gravship's landing area")]
        public async Task Land(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GravshipLandRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _service.Land(body), Applied);
        }
    }
}
