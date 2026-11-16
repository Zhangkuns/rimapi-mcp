using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class OdysseyAnimalWorkController
    {
        private readonly IAnimalWorkControlService _service;
        private readonly AiMutationCoordinator _mutations;

        public OdysseyAnimalWorkController(
            IAnimalWorkControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/odyssey/animal-work")]
        [EndpointMetadata("Read native animal forage/dig policy and learned training")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _service.GetAnimalWork(context.Request.QueryString["pawn_id"]));
        }

        [Post("/api/v1/dlc/odyssey/animal-work")]
        [EndpointMetadata("Set native animal forage/dig policy for a trained colony animal")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AnimalWorkRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetAnimalWork(body),
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
    }
}
