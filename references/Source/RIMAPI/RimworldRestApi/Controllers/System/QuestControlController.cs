// AI-FORK: quest generation with mandatory preview.
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public class QuestControlController
    {
        private readonly AiMutationCoordinator _mutations;
        private readonly IQuestControlService _questControl;

        public QuestControlController(IQuestControlService questControl, AiMutationCoordinator mutations)
        {
            _questControl = questControl;
            _mutations = mutations;
        }

        [Get("/api/v1/quests/options")]
        [EndpointMetadata("Read native quest requirements, accepters and indexed reward choices")]
        public async Task Options(HttpListenerContext context)
        {
            var raw = context.Request.QueryString["quest_id"];
            int? id = null;
            if (!string.IsNullOrEmpty(raw))
            {
                if (!int.TryParse(raw, out var parsed))
                {
                    await ResponseBuilder.SendApiResult(context.Response,
                        ApiResult.Fail("quest_id must be an integer", ApiErrorCode.BadRequest));
                    return;
                }
                id = parsed;
            }
            await _mutations.RunOnMain(() => ResponseBuilder.SendApiResult(
                context.Response, _questControl.GetOptions(id)));
        }

        [Get("/api/v1/dlc/ideology/archonexus")]
        [EndpointMetadata("Read Archonexus quests, native restart selection and cores")]
        public async Task Archonexus(HttpListenerContext context)
        {
            await _mutations.RunOnMain(() => ResponseBuilder.SendApiResult(
                context.Response, _questControl.GetArchonexus()));
        }

        [Post("/api/v1/quests/accept")]
        [EndpointMetadata("Choose native quest rewards and accept with a legal colonist")]
        public async Task Accept(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<QuestAcceptRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _questControl.Accept(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/archonexus/select")]
        [EndpointMetadata("Confirm exact native carry choices and begin the restart cinematic")]
        public async Task SelectArchonexus(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ArchonexusSelectRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _questControl.SelectArchonexus(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/archonexus/tile")]
        [EndpointMetadata("Choose a native-valid restart tile")]
        public async Task ChooseArchonexusTile(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ArchonexusTileRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _questControl.ChooseArchonexusTile(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/archonexus/ideology")]
        [EndpointMetadata("Choose restart ideology and queue native colony migration")]
        public async Task ChooseArchonexusIdeology(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ArchonexusIdeologyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _questControl.ChooseArchonexusIdeology(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/archonexus/cancel")]
        [EndpointMetadata("Cancel the native carry selection before confirmation")]
        public async Task CancelArchonexus(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ArchonexusQuestRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _questControl.CancelArchonexus(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/archonexus/study")]
        [EndpointMetadata("Set the native archotech structure study toggle; research work remains native")]
        public async Task SetArchonexusStudy(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ArchonexusStudyRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _questControl.SetArchonexusStudy(body), Applied);
        }

        [Post("/api/v1/dlc/ideology/archonexus/activate")]
        [EndpointMetadata("Order a colonist to invoke an exact core after reaching it")]
        public async Task ActivateArchonexus(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ArchonexusActivateRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _questControl.ActivateArchonexus(body), Applied);
        }

        private static bool Applied(ApiResult<System.Collections.Generic.Dictionary<string, object>> r) =>
            r.Data != null && r.Data.TryGetValue("applied", out var value) && value is bool b && b;

        [Delete("/api/v1/quests")]
        [EndpointMetadata("Remove a quest by id (dry_run supported)")]
        public async Task Remove(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<QuestRemoveRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun, () => _questControl.Remove(body));
        }

        [Post("/api/v1/quests/generate")]
        [EndpointMetadata("Generate a quest (preview unless confirm=true)")]
        public async Task Generate(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<QuestGenerateRequest>();
            await _mutations.SendAsync(context, body, body != null && (body.DryRun || !body.Confirm), () => _questControl.Generate(body));
        }
    }
}
