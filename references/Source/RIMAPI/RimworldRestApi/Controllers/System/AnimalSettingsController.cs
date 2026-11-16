using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class AnimalSettingsController
    {
        private readonly IAnimalSettingsService _service;
        private readonly AiMutationCoordinator _mutations;

        public AnimalSettingsController(IAnimalSettingsService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/animal/settings")]
        [EndpointMetadata("Read native animal master/follow/release/training settings")]
        public async Task Get(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.Get(context.Request.QueryString["animal_id"]));

        [Post("/api/v1/animal/settings")]
        [EndpointMetadata("Set native animal master/follow/release/attack target")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AnimalSettingsRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.Set(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
