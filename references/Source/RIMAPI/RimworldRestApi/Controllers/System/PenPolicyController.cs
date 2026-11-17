using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;
using RIMAPI.Http;

namespace RIMAPI.Controllers
{
    public class PenPolicyController
    {
        private readonly IPenPolicyService _service;
        private readonly AiMutationCoordinator _mutations;

        public PenPolicyController(IPenPolicyService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/pen/policy")]
        [EndpointMetadata("Read a pen marker's name, auto-cut and filters")]
        public async Task GetPen(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.GetPen(context.Request.QueryString["thing_id"]));

        [Get("/api/v1/animal/pens")]
        [EndpointMetadata("List built animal pens, enclosed cells, animals and hitching spots on a map")]
        public async Task GetPens(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.GetPens(RequestParser.GetMapId(context)));

        [Get("/api/v1/animal/pen")]
        [EndpointMetadata("Read an animal's native pen, rope, hitching and handler job state")]
        public async Task GetAnimalPen(HttpListenerContext context) =>
            await context.SendJsonResponse(_service.GetAnimalPen(context.Request.QueryString["animal_id"]));

        [Post("/api/v1/animal/rope")]
        [EndpointMetadata("Submit native rope-to-pen or rope-to-hitch work with an eligible handler")]
        public async Task RopeAnimal(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RopeAnimalRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.RopeAnimal(body),
                result => result.Data != null && result.Data.ContainsKey("submitted")
                    && (bool)result.Data["submitted"]);
        }

        [Post("/api/v1/pen/policy")]
        [EndpointMetadata("Set pen marker name/auto-cut and plant or animal filters")]
        public async Task SetPen(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PenPolicyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetPen(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }

        [Get("/api/v1/autoslaughter")]
        [EndpointMetadata("List map auto-slaughter configs and current candidates")]
        public async Task GetAutoSlaughter(HttpListenerContext context)
        {
            int? mapId = int.TryParse(context.Request.QueryString["map_id"], out var id) ? id : (int?)null;
            await context.SendJsonResponse(_service.GetAutoSlaughter(mapId));
        }

        [Post("/api/v1/autoslaughter")]
        [EndpointMetadata("Set one animal's auto-slaughter gender/age limits")]
        public async Task SetAutoSlaughter(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AutoSlaughterRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetAutoSlaughter(body),
                result => result.Data != null && result.Data.ContainsKey("applied"));
        }
    }
}
