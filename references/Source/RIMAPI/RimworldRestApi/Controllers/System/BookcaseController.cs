using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public sealed class BookcaseController
    {
        readonly IBookcaseService service;
        readonly AiMutationCoordinator mutations;
        public BookcaseController(IBookcaseService service, AiMutationCoordinator mutations)
        { this.service = service; this.mutations = mutations; }

        [Get("/api/v1/building/books")]
        [EndpointMetadata("List the exact books held by a player bookcase")]
        public async Task Get(HttpListenerContext context) => await context.SendJsonResponse(service.Get(
            context.Request.QueryString["thing_id"], context.Request.QueryString["map_id"] == null ? (int?)null
                : RequestParser.GetIntParameter(context, "map_id")));

        [Post("/api/v1/building/books/eject")]
        [EndpointMetadata("Eject one exact contained book and forbid it, as in the native bookcase tab")]
        public async Task Eject(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BookcaseEjectRequest>();
            await mutations.SendAsync(context, body, body != null && body.DryRun, () => service.Eject(body), r => r.Success);
        }
    }
}
