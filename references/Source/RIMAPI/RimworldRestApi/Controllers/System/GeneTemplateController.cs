using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class GeneTemplateController
    {
        private readonly IGeneTemplateService _service;
        private readonly AiMutationCoordinator _mutations;

        public GeneTemplateController(IGeneTemplateService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/gene/template")]
        [EndpointMetadata("List saved native custom xenogerm templates")]
        public async Task Get(HttpListenerContext context)
        {
            await context.SendJsonResponse(_service.Get());
        }

        [Post("/api/v1/dlc/biotech/gene/template/save")]
        [EndpointMetadata("Save a native custom xenogerm template from connected genepacks")]
        public async Task Save(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneTemplateSaveRequest>();
            await _mutations.SendAsync(
                context, body, body != null && body.DryRun,
                () => _service.Save(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/biotech/gene/template/load")]
        [EndpointMetadata("Resolve a saved template to matching connected genepacks")]
        public async Task Load(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneTemplateLoadRequest>();
            await context.SendJsonResponse(_service.Load(body));
        }
    }
}
