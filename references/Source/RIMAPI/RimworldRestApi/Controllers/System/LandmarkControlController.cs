// AI-FORK: G-42 Odyssey landmark read endpoint.
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class LandmarkControlController
    {
        private readonly ILandmarkControlService _service;

        public LandmarkControlController(ILandmarkControlService service)
        {
            _service = service;
        }

        [Get("/api/v1/dlc/odyssey/landmark/tile")]
        [EndpointMetadata("Read the landmark and mutators at one exact layer/tile pair")]
        public async Task GetTile(HttpListenerContext context)
        {
            var q = context.Request.QueryString;
            await ResponseBuilder.SendApiResult(context.Response, _service.GetTile(q["layer_id"], q["tile_id"]));
        }

        [Get("/api/v1/dlc/odyssey/landmark")]
        [EndpointMetadata("Read surface landmarks and per-def counts")]
        public async Task Get(HttpListenerContext context)
        {
            var raw = context.Request.QueryString["limit"];
            var limit = 100;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out limit) || limit <= 0 || limit > 5000)
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "limit must be an integer between 1 and 5000",
                            ApiErrorCode.BadRequest
                        )
                    );
                    return;
                }
            }
            var offset = 0;
            var rawOffset = context.Request.QueryString["offset"];
            if (rawOffset != null && (!int.TryParse(rawOffset, out offset) || offset < 0))
            {
                await ResponseBuilder.SendApiResult(context.Response, ApiResult<Dictionary<string, object>>.Fail(
                    "offset must be a nonnegative integer", ApiErrorCode.BadRequest));
                return;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.Get(limit, offset, context.Request.QueryString["layer_id"]));
        }
    }
}
