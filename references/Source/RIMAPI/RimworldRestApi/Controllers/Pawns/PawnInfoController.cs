using System.Threading.Tasks;
using System.Net;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class PawnInfoController
    {
        private readonly IPawnInfoService _pawnInfoService;

        public PawnInfoController(IPawnInfoService pawnInfoService)
        {
            _pawnInfoService = pawnInfoService;
        }

        [Get("/api/v1/pawns/details")]
        public async Task GetPawnDetails(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "id");
            var result = _pawnInfoService.GetPawnDetails(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/inventory")]
        public async Task GetPawnInventory(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "id");
            var result = _pawnInfoService.GetPawnInventory(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/gear")]
        [EndpointMetadata("PAWN-Q02: native gear mass, capacity, comfort range, armor totals and item weights")]
        public async Task GetPawnGear(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _pawnInfoService.GetPawnGear(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/bio")]
        [EndpointMetadata("PAWN-Q04: actual age, backstories, work restrictions with sources, trait degree and source")]
        public async Task GetPawnBio(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _pawnInfoService.GetPawnBio(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/needs")]
        [EndpointMetadata("PAWN-Q05: native NeedDef list, food detail and explicit absence vs zero")]
        public async Task GetPawnNeeds(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var needDef = RequestParser.HasParameter(context, "need_def")
                ? RequestParser.GetStringParameter(context, "need_def") : null;
            var result = _pawnInfoService.GetPawnNeeds(pawnId, needDef);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawns/health")]
        [EndpointMetadata("PAWN-Q06: visible capacities, total pain, total bleed rate and bleed-out estimate")]
        public async Task GetPawnHealth(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _pawnInfoService.GetPawnHealth(pawnId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/pawn/surgery/available")]
        [EndpointMetadata("PAWN-Q06: native surgery recipes and applicable body parts for one pawn")]
        public async Task GetAvailableSurgery(HttpListenerContext context)
        {
            int pawnId = RequestParser.GetIntParameter(context, "pawn_id");
            var result = _pawnInfoService.GetAvailableSurgery(pawnId);
            await context.SendJsonResponse(result);
        }
    }
}