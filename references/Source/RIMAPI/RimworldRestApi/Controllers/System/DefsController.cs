// AI-FORK: read-only def queries. Never mutates the Def database.
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class DefsController
    {
        private readonly IDefQueryService _defQueryService;

        public DefsController(IDefQueryService defQueryService)
        {
            _defQueryService = defQueryService;
        }

        [Get("/api/v1/defs/types")]
        [EndpointMetadata("List loaded def types with counts and owning DLC")]
        public async Task GetDefTypes(HttpListenerContext context)
        {
            var result = _defQueryService.GetTypes();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/defs")]
        [EndpointMetadata("List defs of one type (query: type, offset, limit, fields)")]
        public async Task ListDefs(HttpListenerContext context)
        {
            var query = context.Request.QueryString;
            var result = _defQueryService.ListDefs(
                query["type"],
                ParseInt(query["offset"], 0),
                ParseInt(query["limit"], 0),
                query["fields"]
            );
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/defs/detail")]
        [EndpointMetadata("Show one def by defName (query: def_name, fields)")]
        public async Task GetDef(HttpListenerContext context)
        {
            var query = context.Request.QueryString;
            var result = _defQueryService.GetDef(query["def_name"], query["fields"]);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/defs/search")]
        [EndpointMetadata("Search defName/label (query: q, types, limit)")]
        public async Task Search(HttpListenerContext context)
        {
            var query = context.Request.QueryString;
            var result = _defQueryService.Search(
                query["q"],
                query["types"],
                ParseInt(query["limit"], 0)
            );
            await context.SendJsonResponse(result);
        }

        private static int ParseInt(string raw, int fallback)
        {
            int value;
            return int.TryParse(raw, out value) ? value : fallback;
        }
    }
}
