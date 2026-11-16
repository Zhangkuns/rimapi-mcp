// AI-FORK: G-32 ideology style read and single-thing application.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class StyleControlController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IStyleControlService _service;

        public StyleControlController(IStyleControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/ideology/style")]
        [EndpointMetadata("Read a thing's style and the styles applicable to its def")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.Get(context.Request.QueryString["thing_id"])
            );
        }

        [Post("/api/v1/dlc/ideology/style")]
        [EndpointMetadata("Apply one ideo style to a single thing")]
        public async Task Apply(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<StyleApplyRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Apply(body),
                Changed
            );
        }

        private static bool Changed(ApiResult<Dictionary<string, object>> result)
        {
            object changed;
            return result.Data != null
                && result.Data.TryGetValue("changed", out changed)
                && changed is bool
                && (bool)changed;
        }
    }
}
