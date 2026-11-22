using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class CorpseRotController
    {
        private readonly ICorpseRotService _corpseRotService;

        public CorpseRotController(ICorpseRotService corpseRotService)
        {
            _corpseRotService = corpseRotService;
        }

        [Get("/api/v1/map/corpse/rot")]
        [EndpointMetadata("Read native rot stage and time estimates for a spawned corpse")]
        public async Task GetCorpseRot(HttpListenerContext context)
        {
            int mapId = RequestParser.GetMapId(context);
            int thingId = RequestParser.GetIntParameter(context, "thing_id");
            var result = _corpseRotService.GetCorpseRot(mapId, thingId);
            await context.SendJsonResponse(result);
        }
    }
}
