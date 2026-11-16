// AI-FORK: G-28 royalty title read and restricted grant.
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class RoyaltyTitleController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IRoyaltyTitleService _service;

        public RoyaltyTitleController(
            IRoyaltyTitleService service,
            AiMutationCoordinator mutations
        )
        {
            _service = service;
            _mutations = mutations;
        }

        [Get("/api/v1/dlc/royalty/title/room-requirements")]
        [EndpointMetadata("Evaluate the title's native throne/bedroom room requirements")]
        public async Task GetRoomRequirements(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetRoomRequirements(context.Request.QueryString["pawn_id"])
            );
        }

        [Get("/api/v1/dlc/royalty/title")]
        [EndpointMetadata("Read current royal title, seniority and favor")]
        public async Task Get(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.Get(context.Request.QueryString["pawn_id"])
            );
        }

        [Get("/api/v1/dlc/royalty/heir")]
        [EndpointMetadata("Read the current royal heir and the native heir candidate list")]
        public async Task GetHeir(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetHeir(
                    context.Request.QueryString["pawn_id"],
                    context.Request.QueryString["faction_def"]
                )
            );
        }

        [Post("/api/v1/dlc/royalty/heir")]
        [EndpointMetadata("Request a native ChangeRoyalHeir quest for the selected heir")]
        public async Task RequestHeir(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RoyalHeirRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.RequestHeir(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/title/relinquish")]
        [EndpointMetadata("Relinquish a royal title through native SetTitle(null)")]
        public async Task Relinquish(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TitleRelinquishRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Relinquish(body),
                result => result.Data != null
                    && result.Data.TryGetValue("applied", out var applied)
                    && applied is bool && (bool)applied);
        }

        [Post("/api/v1/dlc/royalty/title")]
        [EndpointMetadata("Grant a royal title; rewards and downgrades are opt-in")]
        public async Task Grant(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<TitleGrantRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.Grant(body),
                Changed
            );
        }

        [Get("/api/v1/dlc/royalty/bestowing/status")]
        [EndpointMetadata("Read bestowing ceremony quest eligibility and the current quest")]
        public async Task GetBestowingStatus(HttpListenerContext context)
        {
            await ResponseBuilder.SendApiResult(
                context.Response,
                _service.GetBestowingStatus(context.Request.QueryString["pawn_id"])
            );
        }

        [Post("/api/v1/dlc/royalty/bestowing/quest")]
        [EndpointMetadata("Generate the native bestowing ceremony quest when eligible")]
        public async Task GenerateBestowingQuest(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BestowingQuestRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.GenerateBestowingQuest(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/decree")]
        [EndpointMetadata("Issue a native royal decree quest for a titled pawn")]
        public async Task IssueDecree(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<RoyalDecreeRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.IssueDecree(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/bestowing/quest/accept")]
        [EndpointMetadata("Accept the pending native bestowing ceremony quest")]
        public async Task AcceptBestowingQuest(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BestowingQuestAcceptRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.AcceptBestowingQuest(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/bestowing/ceremony/start")]
        [EndpointMetadata("Start the native bestowing ceremony when the quest bestower is waiting")]
        public async Task StartBestowingCeremony(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BestowingCeremonyStartRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.StartBestowingCeremony(body),
                Applied
            );
        }

        [Post("/api/v1/dlc/royalty/bestowing/ceremony/cancel")]
        [EndpointMetadata("Cancel the current native bestowing ceremony as failed")]
        public async Task CancelBestowingCeremony(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<BestowingCeremonyCancelRequest>();
            await _mutations.SendAsync(
                context,
                body,
                body != null && body.DryRun,
                () => _service.CancelBestowingCeremony(body),
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
