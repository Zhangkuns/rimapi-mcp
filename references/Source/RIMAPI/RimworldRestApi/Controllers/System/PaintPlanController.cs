using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PaintPlanController
    {
        private readonly IPaintPlanService _service;
        public PaintPlanController(IPaintPlanService service) { _service = service; }

        [Get("/api/v1/map/paint")]
        [EndpointMetadata("Read floor/building paint and native paint designations")]
        public async Task GetPaint(HttpListenerContext context)
        {
            int? x = int.TryParse(context.Request.QueryString["x"], out var xv) ? xv : (int?)null;
            int? z = int.TryParse(context.Request.QueryString["z"], out var zv) ? zv : (int?)null;
            await context.SendJsonResponse(
                _service.GetPaint(x, z, context.Request.QueryString["thing_id"]));
        }

        [Post("/api/v1/map/paint")]
        [EndpointMetadata("Add/remove a native paint designation (work giver consumes Dye)")]
        public async Task SetPaint(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PaintRequest>();
            await context.SendJsonResponse(_service.SetPaint(body));
        }

        [Get("/api/v1/map/plans")]
        [EndpointMetadata("Read all native PlanManager plans with exact cells")]
        public async Task ListPlans(HttpListenerContext context)
        {
            await context.SendJsonResponse(_service.GetPlans());
        }

        [Post("/api/v1/map/plans")]
        [EndpointMetadata("Create/edit/copy/delete native PlanManager plans")]
        public async Task ControlPlans(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PlanRequest>();
            await context.SendJsonResponse(_service.ControlPlans(body));
        }
    }
}
