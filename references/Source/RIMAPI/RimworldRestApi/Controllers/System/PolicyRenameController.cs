using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PolicyRenameController
    {
        readonly IPolicyRenameService service;
        readonly AiMutationCoordinator mutations;
        public PolicyRenameController(IPolicyRenameService service, AiMutationCoordinator mutations)
        { this.service = service; this.mutations = mutations; }

        [Post("/api/v1/outfit/policy/rename")]
        [EndpointMetadata("Rename an existing apparel policy by ID, preserving its filter and assignments")]
        public Task Outfit(HttpListenerContext context) => Rename(context, "outfit");

        [Post("/api/v1/food/policy/rename")]
        [EndpointMetadata("Rename an existing food policy by ID")]
        public Task Food(HttpListenerContext context) => Rename(context, "food");

        [Post("/api/v1/drug/policy/rename")]
        [EndpointMetadata("Rename an existing drug policy by ID")]
        public Task Drug(HttpListenerContext context) => Rename(context, "drug");

        [Post("/api/v1/reading/policy/rename")]
        [EndpointMetadata("Rename an existing reading policy by ID")]
        public Task Reading(HttpListenerContext context) => Rename(context, "reading");

        async Task Rename(HttpListenerContext context, string kind)
        {
            var body = await context.Request.ReadBodyAsync<PolicyRenameRequest>();
            await mutations.SendAsync(context, body, body != null && body.DryRun,
                () => service.Rename(kind, body), r => r.Data != null && r.Data.ContainsKey("applied") && (bool)r.Data["applied"]);
        }
    }
}
