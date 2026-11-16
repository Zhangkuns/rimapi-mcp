// AI-FORK: G-41 Odyssey planet layer read endpoint.
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PlanetLayerControlController
    {
        private readonly IPlanetLayerControlService _service;

        public PlanetLayerControlController(IPlanetLayerControlService service)
        {
            _service = service;
        }

        [Get("/api/v1/dlc/odyssey/layers/tile")]
        [EndpointMetadata("Read an exact layer/tile identity and optional native closest tile on another layer; does not select a view or move anything")]
        public async Task GetTile(HttpListenerContext context)
        {
            var q = context.Request.QueryString;
            await ResponseBuilder.SendApiResult(context.Response, _service.GetTile(q["layer_id"], q["tile_id"], q["target_layer_id"]));
        }

        [Get("/api/v1/dlc/odyssey/layers")]
        [EndpointMetadata("Read planet layers, sizes and the current map layer")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response, _service.Get());
        }
    }
}
