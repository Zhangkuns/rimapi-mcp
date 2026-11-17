using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;

namespace RIMAPI.Controllers
{
    public sealed class PreferencesController
    {
        readonly IPreferencesService service;
        readonly AiMutationCoordinator mutations;
        public PreferencesController(IPreferencesService service, AiMutationCoordinator mutations)
        { this.service = service; this.mutations = mutations; }

        [Get("/api/v1/game/preferences")]
        [EndpointMetadata("Read native UI preferences, legal values, available languages and display modes")]
        public async Task Preferences(HttpListenerContext context) => await context.SendJsonResponse(service.GetPreferences());

        [Post("/api/v1/game/preferences")]
        [EndpointMetadata("Apply and save whitelisted native UI preferences; available in the main menu")]
        public async Task SetPreferences(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PreferencesRequest>();
            await mutations.SendEnvironmentAsync(context, body, body != null && body.DryRun,
                () => service.SetPreferences(body), r => r.Success);
        }

        [Get("/api/v1/game/play-settings")]
        [EndpointMetadata("Read native map/world display flags")]
        public async Task PlaySettings(HttpListenerContext context) => await context.SendJsonResponse(service.GetPlaySettings());

        [Post("/api/v1/game/play-settings")]
        [EndpointMetadata("Set native map/world display flags for the active Game")]
        public async Task SetPlaySettings(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<PlaySettingsRequest>();
            await mutations.SendAsync(context, body, body != null && body.DryRun,
                () => service.SetPlaySettings(body), r => r.Success);
        }

        [Get("/api/v1/game/key-bindings")]
        [EndpointMetadata("List native key bindings, both slots, defaults and valid key codes")]
        public async Task KeyBindings(HttpListenerContext context) => await context.SendJsonResponse(service.GetKeyBindings());

        [Post("/api/v1/game/key-bindings")]
        [EndpointMetadata("Set, clear or reset native key bindings and save KeyPrefs")]
        public async Task SetKeyBindings(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<KeyBindingsRequest>();
            await mutations.SendEnvironmentAsync(context, body, body != null && body.DryRun,
                () => service.SetKeyBindings(body), r => r.Success);
        }
    }
}
