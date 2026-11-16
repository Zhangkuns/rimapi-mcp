// AI-FORK: Phase 2a HTTP surface — areas, conditions, apparel, surgery.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class Phase2Controller
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IMapControlService _mapControl;
        private readonly IPawnControlService _pawnControl;

        public Phase2Controller(
            IMapControlService mapControl,
            IPawnControlService pawnControl,
            AiMutationCoordinator mutations
        )
        {
            _mutations = mutations;
            _mapControl = mapControl;
            _pawnControl = pawnControl;
        }

        // ----- areas -----

        [Get("/api/v1/map/areas")]
        [EndpointMetadata("List map areas with cell counts and assignability")]
        public async Task GetAreas(HttpListenerContext context)
        {
            var mapId = ParseInt(context.Request.QueryString["map_id"], 0);
            await context.SendJsonResponse(_mapControl.GetAreas(mapId));
        }

        [Post("/api/v1/map/areas")]
        [EndpointMetadata("Create a new allowed area (label)")]
        public async Task CreateArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MapAreaRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _mapControl.CreateArea(body));
        }

        [Delete("/api/v1/map/areas")]
        [EndpointMetadata("Delete an allowed area and clear pawn assignments to it")]
        public async Task DeleteArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MapAreaRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _mapControl.DeleteArea(body));
        }

        [Post("/api/v1/map/areas/cells")]
        [EndpointMetadata("Set or clear area cells (cells[] or rect, value)")]
        public async Task SetAreaCells(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AreaCellsRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _mapControl.SetAreaCells(body));
        }

        [Post("/api/v1/map/areas/edit")]
        [EndpointMetadata("Rename/color/invert/clear/delete an allowed area")]
        public async Task EditArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AreaEditRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _mapControl.EditArea(body));
        }

        [Get("/api/v1/map/area-policies")]
        [EndpointMetadata("Read Home/BuildRoof/NoRoof/IgnoreRoof areas and auto flags")]
        public async Task GetAreaPolicies(HttpListenerContext context)
        {
            await context.SendJsonResponse(
                _mapControl.GetAreaPolicies(ParseInt(context.Request.QueryString["map_id"], 0)));
        }

        [Post("/api/v1/map/area-policies")]
        [EndpointMetadata("Set auto-home-area/auto-rebuild policy flags")]
        public async Task SetAreaPolicies(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AreaPolicyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _mapControl.SetAreaPolicies(body));
        }

        [Post("/api/v1/pawn/area")]
        [EndpointMetadata("Assign a pawn to an area (null area = unrestricted)")]
        public async Task AssignPawnArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PawnAreaRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _mapControl.AssignPawnArea(body));
        }

        // ----- game conditions -----

        [Get("/api/v1/map/conditions")]
        [EndpointMetadata("List active game conditions with remaining ticks")]
        public async Task GetConditions(HttpListenerContext context)
        {
            var mapId = ParseInt(context.Request.QueryString["map_id"], 0);
            await context.SendJsonResponse(_mapControl.GetConditions(mapId));
        }

        [Post("/api/v1/map/conditions")]
        [EndpointMetadata("Start a GameCondition (condition_def, duration_ticks or duration_days)")]
        public async Task StartCondition(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GameConditionRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _mapControl.StartCondition(body));
        }

        [Delete("/api/v1/map/conditions")]
        [EndpointMetadata("End a game condition (all=true ends every active condition)")]
        public async Task EndCondition(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<EndConditionRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _mapControl.EndCondition(body));
        }

        // ----- apparel -----

        [Get("/api/v1/pawn/apparel")]
        [EndpointMetadata("List worn apparel with forced flags")]
        public async Task GetApparel(HttpListenerContext context)
        {
            var pawnId = context.Request.QueryString["pawn_id"];
            await context.SendJsonResponse(_pawnControl.GetApparel(pawnId));
        }

        [Post("/api/v1/pawn/apparel/wear")]
        [EndpointMetadata("Make a pawn wear an apparel item (thing_id)")]
        public async Task Wear(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ApparelActionRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _pawnControl.Wear(body));
        }

        [Post("/api/v1/pawn/apparel/remove")]
        [EndpointMetadata("Remove a worn apparel item (thing_id)")]
        public async Task RemoveApparel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ApparelActionRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _pawnControl.RemoveApparel(body));
        }

        [Post("/api/v1/pawn/apparel/force")]
        [EndpointMetadata("Force or unforce an apparel item in the pawn's outfit")]
        public async Task ForceApparel(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ApparelForceRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _pawnControl.ForceApparel(body));
        }

        // ----- surgery -----

        [Get("/api/v1/pawn/surgery")]
        [EndpointMetadata("List queued surgery bills")]
        public async Task GetSurgery(HttpListenerContext context)
        {
            var pawnId = context.Request.QueryString["pawn_id"];
            await context.SendJsonResponse(_pawnControl.GetSurgery(pawnId));
        }

        [Get("/api/v1/pawn/surgery/options")]
        [EndpointMetadata("List exact body parts a surgery recipe can target on this pawn")]
        public async Task SurgeryOptions(HttpListenerContext context)
        {
            await context.SendJsonResponse(_pawnControl.SurgeryOptions(
                context.Request.QueryString["pawn_id"],
                context.Request.QueryString["recipe_def"]));
        }

        [Post("/api/v1/pawn/surgery/bill")]
        [EndpointMetadata("Suspend/resume or reorder one exact surgery bill")]
        public async Task ManageSurgeryBill(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SurgeryBillRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _pawnControl.ManageSurgeryBill(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/pawn/surgery")]
        [EndpointMetadata("Queue a surgery bill (recipe_def, body_part); a doctor performs it")]
        public async Task AddSurgery(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SurgeryRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _pawnControl.AddSurgery(body));
        }

        [Delete("/api/v1/pawn/surgery")]
        [EndpointMetadata("Remove queued surgery bills (recipe_def optional)")]
        public async Task RemoveSurgery(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SurgeryRemoveRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _pawnControl.RemoveSurgery(body));
        }

        // ----- item use -----

        [Post("/api/v1/pawn/use-item")]
        [EndpointMetadata("Use an item through the native CompUsable path; target_thing_id for CompTargetable items")]
        public async Task UseItem(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<UseItemRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _pawnControl.UseItem(body));
        }

        // ----- abilities -----

        [Get("/api/v1/pawn/ability")]
        [EndpointMetadata("List the native abilities this pawn holds with cooldown/charges")]
        public async Task GetAbilities(HttpListenerContext context)
        {
            var pawnId = context.Request.QueryString["pawn_id"];
            await context.SendJsonResponse(_pawnControl.GetAbilities(pawnId));
        }

        [Post("/api/v1/pawn/ability")]
        [EndpointMetadata("Use a native ability through Ability.GetJob (pawn/thing/cell/self, optional dest_cell)")]
        public async Task UseAbility(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AbilityRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _pawnControl.UseAbility(body));
        }

        // ----- Odyssey outfit stand -----

        [Get("/api/v1/dlc/odyssey/outfit-stands")]
        [EndpointMetadata("List native outfit stands and the apparel/weapon they hold")]
        public async Task GetOutfitStands(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await context.SendJsonResponse(
                        ApiResult<List<Dictionary<string, object>>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await context.SendJsonResponse(_pawnControl.GetOutfitStands(mapId));
        }

        [Post("/api/v1/dlc/odyssey/outfit-stand/use")]
        [EndpointMetadata("Use a native outfit stand to swap the pawn's apparel")]
        public async Task UseOutfitStand(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<OutfitStandUseRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _pawnControl.UseOutfitStand(body));
        }

        [Post("/api/v1/dlc/odyssey/outfit-stand/manage")]
        [EndpointMetadata("Manage the native outfit stand: hauling flag, put apparel, or eject one item")]
        public async Task ManageOutfitStand(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<OutfitStandManageRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _pawnControl.ManageOutfitStand(body),
                Applied
            );
        }

        private static bool Applied(ApiResult<Dictionary<string, object>> result)
        {
            object applied;
            return result.Data != null
                && result.Data.TryGetValue("applied", out applied)
                && applied is bool
                && (bool)applied;
        }

        private static int ParseInt(string raw, int fallback)
        {
            int value;
            return int.TryParse(raw, out value) ? value : fallback;
        }
    }
}
