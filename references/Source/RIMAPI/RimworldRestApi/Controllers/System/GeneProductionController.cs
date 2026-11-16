// AI-FORK: Biotech normal gene production endpoints.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class GeneProductionController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IGeneProductionService _service;

        public GeneProductionController(IGeneProductionService service, AiMutationCoordinator mutations)
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/biotech/gene/extractor")]
        [EndpointMetadata("Read gene extractors: power, working, contained pawn and ticks remaining")]
        public async Task GetExtractors(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetExtractors(mapId));
        }

        [Post("/api/v1/dlc/biotech/gene/extract/cancel")]
        [EndpointMetadata("Cancel a selected donor or an active native gene extraction")]
        public async Task CancelExtract(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneExtractCancelRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.CancelExtract(body),
                Applied);
        }

        [Post("/api/v1/dlc/biotech/gene/reimplant")]
        [EndpointMetadata("Native xenogerm reimplantation: an absorber takes a downed/secure donor's xenogenes")]
        public async Task ReimplantXenogerm(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneReimplantRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.ReimplantXenogerm(body),
                Applied
            );
        }

                [Post("/api/v1/dlc/biotech/gene/extract")]
        [EndpointMetadata("Order a pawn into a gene extractor (native SelectedPawn + EnterBuilding)")]
        public async Task Extract(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneExtractRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Extract(body),
                Applied);
        }

        [Post("/api/v1/dlc/biotech/ovum/fertilize")]
        [EndpointMetadata("Order a native FertilizeOvum job for an exact ovum and father")]
        public async Task FertilizeOvum(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<FertilizeOvumRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.FertilizeOvum(body),
                Applied);
        }

        [Get("/api/v1/dlc/biotech/gene/bank")]
        [EndpointMetadata("Read gene banks: autoLoad, contents and pending load list")]
        public async Task GetGenebanks(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetGenebanks(mapId));
        }

        [Post("/api/v1/dlc/biotech/gene/bank")]
        [EndpointMetadata("Set gene bank autoLoad or eject one/all native genepacks")]
        public async Task SetGenebank(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GenebankRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.SetGenebank(body),
                Applied);
        }

        [Get("/api/v1/dlc/biotech/gene/assembler")]
        [EndpointMetadata("Read gene assemblers and the genepacks in connected gene banks")]
        public async Task GetAssemblers(HttpListenerContext context)
        {
            int? mapId = null;
            var raw = context.Request.QueryString["map_id"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(
                        context.Response,
                        ApiResult<Dictionary<string, object>>.Fail(
                            "map_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                mapId = parsed;
            }
            await ResponseBuilder.SendApiResult(context.Response, _service.GetAssemblers(mapId));
        }

        [Post("/api/v1/dlc/biotech/gene/assemble/cancel")]
        [EndpointMetadata("Cancel the working native gene assembly (same Reset as CancelXenogerm)")]
        public async Task CancelAssemble(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneAssembleCancelRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.CancelAssemble(body),
                Applied);
        }

        [Post("/api/v1/dlc/biotech/gene/assemble")]
        [EndpointMetadata("Start a xenogerm recombination from genepacks in connected gene banks")]
        public async Task Assemble(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GeneAssembleRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Assemble(body),
                Applied);
        }

        private static bool Applied(ApiResult<Dictionary<string, object>> result)
        {
            object applied;
            return result.Data != null
                && result.Data.TryGetValue("applied", out applied)
                && applied is bool
                && (bool)applied;
        }
    }
}
