// AI-FORK: Biotech mechanitor endpoints. Binding queues a native job; the response
// says queued and GET remains the only proof of an actual overseer relation.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class MechControlController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IMechControlService _service;

        public MechControlController(IMechControlService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/bossgroup")]
        [EndpointMetadata("Read bossgroup call counts and the last called tick")]
        public async Task GetBossgroup(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(context.Response, _service.GetBossgroup());
        }

        [Get("/api/v1/dlc/biotech/mech")]
        [EndpointMetadata("Read mechanitor bandwidth, player mechs and control groups")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.Get(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/biotech/mech/bind")]
        [EndpointMetadata("Queue the native ControlMech job for an unbound player mech")]
        public async Task Bind(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechBindRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Bind(body),
                Changed
            );
        }

        [Get("/api/v1/dlc/biotech/mech/gestator")]
        [EndpointMetadata("Read mech gestators, bills, gestation state and contents")]
        public async Task GetMechGestator(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer",
                            ApiErrorCode.BadRequest
                        )
                    );
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetMechGestator(mapId));
        }

        [Get("/api/v1/dlc/biotech/mech/carrier")]
        [EndpointMetadata("Read mech carrier resource threshold, contents and release state")]
        public async Task GetMechCarrier(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer",
                            ApiErrorCode.BadRequest
                        )
                    );
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetMechCarrier(context.Request.QueryString["mech_id"], mapId)
            );
        }

        [Post("/api/v1/dlc/biotech/mech/carrier/autofill")]
        [EndpointMetadata("Set the native mech carrier autofill target")]
        public async Task SetMechCarrierAutofill(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechCarrierAutofillRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetMechCarrierAutofill(body),
                Changed
            );
        }

        [Post("/api/v1/dlc/biotech/mech/carrier/release")]
        [EndpointMetadata("Release the native mech carrier's formed mechanoids")]
        public async Task ReleaseMechCarrier(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechCarrierReleaseRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReleaseMechCarrier(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/gestator/haul")]
        [EndpointMetadata("Queue the native haul-to-container job for a mech corpse")]
        public async Task HaulToMechGestator(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechGestatorHaulRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.HaulToMechGestator(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/gestator/eject")]
        [EndpointMetadata("Eject a formed mech from its gestator onto the map")]
        public async Task EjectMechGestator(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechGestatorEjectRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.EjectMechGestator(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/gestator/start")]
        [EndpointMetadata("Queue the native DoBill job to start a mech gestation bill")]
        public async Task StartMechGestator(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechGestatorStartRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.StartMechGestator(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/mech/resurrect/ability")]
        [EndpointMetadata("Read a pawn's native mech resurrection ability and charges")]
        public async Task GetMechResurrectAbility(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetMechResurrectAbility(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/biotech/mech/resurrect")]
        [EndpointMetadata("Queue the native mech resurrection ability on a player mech corpse")]
        public async Task ResurrectMech(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechResurrectRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ResurrectMech(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/release")]
        [EndpointMetadata("Disconnect one controlled mech from its mechanitor")]
        public async Task ReleaseMech(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechReleaseRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReleaseMech(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/repair")]
        [EndpointMetadata("Order a drafted mechanitor to repair a damaged player mech")]
        public async Task RepairMech(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechRepairRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.RepairMech(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/disassemble")]
        [EndpointMetadata("Order the overseer to disassemble one controlled player mech")]
        public async Task DisassembleMech(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechDisassembleRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.DisassembleMech(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/charge")]
        [EndpointMetadata("Queue the native HaulMechToCharger job for a self-shutdown mech")]
        public async Task ChargeMech(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechChargeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ChargeMech(body),
                Applied
            );
        }

        [Get("/api/v1/dlc/biotech/mech/groups")]
        [EndpointMetadata("Read native mechanitor control groups, members and recharge thresholds")]
        public async Task GetGroups(HttpListenerContext context)
        {
            var pawnId = context.Request.QueryString["pawn_id"];
            await ResponseBuilder.SendApiResult(context.Response, _service.GetGroups(pawnId));
        }

        [Post("/api/v1/dlc/biotech/mech/group/assign")]
        [EndpointMetadata("Move an overseen mech into an existing native control group")]
        public async Task AssignGroup(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechGroupAssignRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.AssignGroup(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/group/recharge")]
        [EndpointMetadata("Write the native per-group recharge thresholds")]
        public async Task SetGroupRecharge(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechGroupRechargeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetGroupRecharge(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/biotech/mech/mode")]
        [EndpointMetadata("Set the work mode of one mechanitor control group")]
        public async Task SetMode(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<MechModeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetMode(body),
                Changed
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

        private static bool Changed(ApiResult<Dictionary<string, object>> result)
        {
            object changed;
            return result.Data != null
                && result.Data.TryGetValue("changed", out changed)
                && changed is bool
                && (bool)changed;
        }
    }
}
