using System;
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
        public Task GetPen(HttpListenerContext context) =>
            Read(context, () => _service.GetPen(context.Request.QueryString["thing_id"]));

        [Get("/api/v1/animal/pens")]
        [EndpointMetadata("List built animal pens, enclosed cells, animals and hitching spots on a map")]
        public Task GetPens(HttpListenerContext context) =>
            Read(context, () => _service.GetPens(RequestParser.GetMapId(context)));

        [Get("/api/v1/animal/pen")]
        [EndpointMetadata("Read an animal's native pen, rope, hitching and handler job state")]
        public Task GetAnimalPen(HttpListenerContext context) =>
            Read(context, () => _service.GetAnimalPen(context.Request.QueryString["animal_id"]));

        [Get("/api/v1/pen/food")]
        [EndpointMetadata("Read native pen nutrition growth, consumption, feed stock and example capacities")]
        public Task GetPenFood(HttpListenerContext context) =>
            Read(context, () => _service.GetPenFood(context.Request.QueryString["thing_id"]));

        [Post("/api/v1/pen/food/example")]
        [EndpointMetadata("Add or remove a native pen Food tab example species")]
        public async Task SetPenFoodExample(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PenFoodExampleRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetPenFoodExample(body), WasChanged);
        }

        [Get("/api/v1/animal/production")]
        [EndpointMetadata("Read one animal's life stage and native milk, wool and egg production state")]
        public Task GetAnimalProduction(HttpListenerContext context) =>
            Read(context, () => _service.GetAnimalProduction(context.Request.QueryString["animal_id"]));

        [Get("/api/v1/animal/designation")]
        [EndpointMetadata("Read an animal's native tame and manual-slaughter marks and eligibility")]
        public Task GetAnimalDesignation(HttpListenerContext context) =>
            Read(context, () => _service.GetAnimalDesignation(context.Request.QueryString["animal_id"]));

        [Post("/api/v1/animal/designation")]
        [EndpointMetadata("Add or remove an eligible animal's native tame or manual-slaughter mark")]
        public async Task SetAnimalDesignation(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AnimalDesignationRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _service.SetAnimalDesignation(body), WasChanged);
        }

        // These query parameters select the subject, not rows in a dictionary payload.
        private Task Read(HttpListenerContext context, Func<ApiResult<Dictionary<string, object>>> read) =>
            _mutations.RunOnMain(() => ResponseBuilder.SendApiResult(context.Response, read()));

        private static bool WasChanged(ApiResult<Dictionary<string, object>> result) =>
            result.Data != null && result.Data.TryGetValue("changed", out var changed) && changed is true;

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
        public Task GetAutoSlaughter(HttpListenerContext context)
        {
            int? mapId = int.TryParse(context.Request.QueryString["map_id"], out var id) ? id : (int?)null;
            return Read(context, () => _service.GetAutoSlaughter(mapId));
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
