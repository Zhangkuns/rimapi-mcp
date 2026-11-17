using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class TimetableController
    {
        readonly ITimetableService service;
        readonly AiMutationCoordinator mutations;
        public TimetableController(ITimetableService service, AiMutationCoordinator mutations)
        { this.service = service; this.mutations = mutations; }

        [Get("/api/v1/colonists/schedules")]
        [EndpointMetadata("Read complete 24-hour native timetables for one or all colonists")]
        public async Task Get(HttpListenerContext context)
        {
            int? map = context.Request.QueryString["map_id"] == null ? (int?)null
                : RequestParser.GetIntParameter(context, "map_id");
            await context.SendJsonResponse(service.Get(context.Request.QueryString["pawn_id"], map));
        }

        [Post("/api/v1/colonists/schedules")]
        [EndpointMetadata("Batch native timetable edits for hours, ranges or whole days with actual readback")]
        public async Task Set(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TimetableBatchRequest>();
            await mutations.SendAsync(context, body, body != null && body.DryRun,
                () => service.Set(body), result => result.Data != null
                    && result.Data.ContainsKey("applied") && (bool)result.Data["applied"]);
        }
    }
}
