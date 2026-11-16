using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Helpers;
using RIMAPI.Http;
using RIMAPI.Models;
using RIMAPI.Services;
using RIMAPI.Services.Interfaces;
using RimWorld;

namespace RIMAPI.Controllers
{
    public class GameController
    {
        private readonly IGameStateService _gameStateService;
        private readonly IGameDataService _gameDataService;
        private readonly RIMAPI_Settings _settings;
        private readonly ICachingService _cachingService;
        private readonly IModService _modService;
        private readonly IUIService _uiService;
        private readonly ISelectionService _selectionService;
        private readonly AiMutationCoordinator _mutations;

        public GameController(
            IGameStateService gameStateService,
            IGameDataService gameDataService,
            RIMAPI_Settings settings,
            ICachingService cachingService,
            IModService modService,
            IUIService uiService,
            ISelectionService selectionService,
            AiMutationCoordinator mutations
        )
        {
            _gameStateService = gameStateService;
            _gameDataService = gameDataService;
            _settings = settings;
            _cachingService = cachingService;
            _modService = modService;
            _uiService = uiService;
            _selectionService = selectionService;
            _mutations = mutations;
        }

        [Get("/api/v1/version")]
        [EndpointMetadata("Get versions of: game, mod, API")]
        public async Task GetVersion(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/version",
                dataFactory: () => Task.FromResult(ApiResult<VersionDto>.Ok(
                    new VersionDto
                    {
                        RimWorldVersion = VersionControl.CurrentVersionString,
                        ModVersion = _settings.version,
                        ApiVersion = _settings.apiVersion,
                    }
                )),
                expiration: TimeSpan.FromMinutes(5),
                priority: CachePriority.Low,
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/game/state")]
        public async Task GetGameState(HttpListenerContext context)
        {
            var result = _gameStateService.GetGameState();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/mods/configure")]
        public async Task ConfigureMods(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<ConfigureModsRequestDto>();
            var result = _modService.ConfigureMods(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/mods/list")]
        [EndpointMetadata("Get list of active mods or single mod info if package_id/uid is provided")]
        [ResponseExample(typeof(ApiResponse<List<ModInfoDto>>))]
        public async Task GetModsInfo(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/mods/list",
                dataFactory: () => Task.FromResult(_modService.GetModsInfo()),
                expiration: TimeSpan.FromMinutes(1),
                priority: CachePriority.Normal,
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Get("/api/v1/mods/info")]
        [EndpointMetadata("Get single mod info by package_id or uid")]
        [ResponseExample(typeof(ApiResponse<ModInfoDto>))]
        public async Task GetModInfo(HttpListenerContext context)
        {
            string packageId = RequestParser.HasParameter(context, "package_id")
                ? RequestParser.GetStringParameter(context, "package_id")
                : RequestParser.GetStringParameter(context, "uid");

            var result = _modService.GetModInfo(packageId);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/mods/preview")]
        [EndpointMetadata("Get mod preview image as base64")]
        [ResponseExample(typeof(ApiResponse<string>))]
        public async Task GetModPreview(HttpListenerContext context)
        {
            string packageId = RequestParser.HasParameter(context, "package_id")
                ? RequestParser.GetStringParameter(context, "package_id")
                : RequestParser.GetStringParameter(context, "uid");

            var result = _modService.GetModPreview(packageId);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/select-area")]
        [EndpointMetadata("Select an area on the map")]
        public async Task SelectArea(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SelectAreaRequestDto>();
            var result = _selectionService.SelectArea(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/select")]
        [EndpointMetadata("Select game object")]
        public async Task Select(HttpListenerContext context)
        {
            var objType = RequestParser.GetStringParameter(context, "type");
            var id = RequestParser.GetIntParameter(context, "id");
            var result = _selectionService.Select(objType, id);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/deselect")]
        [EndpointMetadata("Clear game selection")]
        public async Task DeselectAll(HttpListenerContext context)
        {
            var result = _selectionService.DeselectAll();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/open-tab")]
        [EndpointMetadata("Open interface tab")]
        public async Task OpenTab(HttpListenerContext context)
        {
            var tabName = RequestParser.GetStringParameter(context, "name");
            var result = _uiService.OpenTab(tabName);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/def/all")]
        [EndpointMetadata("Get all game definitions with optional filtering")]
        public async Task GetAllDefs(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<AllDefsRequestDto>();

            // Support filters via query string if body is empty (e.g., ?include=things_defs,biome_defs)
            if (body == null || body.Filters == null || body.Filters.Count == 0)
            {
                var filterParam = context.Request.QueryString["include"] ?? context.Request.QueryString["filter"];
                if (!string.IsNullOrEmpty(filterParam))
                {
                    body = new AllDefsRequestDto
                    {
                        Filters = filterParam.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim())
                                            .ToList()
                    };
                }
            }

            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/def/all",
                dataFactory: () => Task.FromResult(_gameDataService.GetAllDefs(body)),
                expiration: TimeSpan.FromMinutes(5),
                priority: CachePriority.Normal,
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Post("/api/v1/game/send/letter")]
        public async Task PostLetter(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<SendLetterRequestDto>();
            var result = _uiService.SendLetterSimple(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/speed")]
        public async Task SetGameSpeed(HttpListenerContext context)
        {
            var speed = RequestParser.GetIntParameter(context, "speed");
            var result = _gameStateService.SetGameSpeed(speed);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/game/game-over")]
        [EndpointMetadata("Read native game-over continuation state and wanderer options")]
        public async Task GetGameOver(HttpListenerContext context)
        {
            await context.SendJsonResponse(_gameStateService.GetGameOver());
        }

        [Get("/api/v1/game/ending")]
        [EndpointMetadata("Read native victory countdown, result and permitted continuation destination")]
        public async Task GetEnding(HttpListenerContext context)
        {
            await context.SendJsonResponse(_gameStateService.GetEnding());
        }

        [Post("/api/v1/game/ending/finish")]
        [EndpointMetadata("Finish native victory credits and follow the ending's original continuation destination")]
        public async Task FinishEnding(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GameEndingRequest>();
            await _mutations.SendAsync(context, body, body != null && body.DryRun,
                () => _gameStateService.FinishEnding(body),
                r => r.Data != null && r.Data.TryGetValue("applied", out var applied) && applied is bool b && b);
        }

        [Post("/api/v1/game/game-over/wanderers/prepare")]
        [EndpointMetadata("Open the native new-wanderer candidate dialog")]
        public async Task PrepareNewWanderers(HttpListenerContext context)
        {
            await context.SendJsonResponse(_gameStateService.PrepareNewWanderers());
        }

        [Post("/api/v1/game/game-over/wanderers")]
        [EndpointMetadata("Confirm native new-wanderer candidates and resume the colony")]
        public async Task ConfirmNewWanderers(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GameOverWandererRequest>();
            await context.SendJsonResponse(_gameStateService.ConfirmNewWanderers(body));
        }

        [Get("/api/v1/game/saves")]
        [EndpointMetadata("Read native save files, metadata and load/save completion")]
        public async Task GetSavedGames(HttpListenerContext context)
        {
            await context.SendJsonResponse(_gameStateService.GetSavedGames());
        }

        [Post("/api/v1/game/save")]
        public async Task GameSave(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GameSaveRequestDto>();
            var result = _gameStateService.GameSave(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/load")]
        public async Task GameLoad(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GameLoadRequestDto>();
            var result = _gameStateService.GameLoad(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/main-menu")]
        [EndpointMetadata("Return to the main menu from an active game")]
        public async Task GoToMainMenu(HttpListenerContext context)
        {
            var result = _gameStateService.GoToMainMenu();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/quit")]
        [EndpointMetadata("Completely close and exit the RimWorld application")]
        public async Task QuitGame(HttpListenerContext context)
        {
            var result = _gameStateService.QuitGame();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/start/devquick")]
        public async Task GameDevQuickStart(HttpListenerContext context)
        {
            var result = _gameStateService.GameDevQuickStart();
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/start/world/preview")]
        [EndpointMetadata("Generate the preview world and legal starting tiles without a map")]
        public async Task PreviewNewGameWorld(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<NewGameWorldPreviewRequestDto>();
            var result = _gameStateService.PreviewNewGameWorld(body);
            await context.SendJsonResponse(result);
        }

        [Post("/api/v1/game/names")]
        [EndpointMetadata("Rename the player faction or a player settlement with native naming rules")]
        public async Task RenameGameNames(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GameNamesRequestDto>();
            var result = _gameStateService.RenameGameNames(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/game/start/options")]
        [EndpointMetadata("Read legal new-game scenarios, world parameters and xenotype presets")]
        public async Task GetNewGameOptions(HttpListenerContext context)
        {
            await context.SendJsonResponse(_gameStateService.GetNewGameStartOptions());
        }

        [Get("/api/v1/game/start/ideo/options")]
        [EndpointMetadata("Read initial new-game ideology presets, memes, styles and modes")]
        public async Task GetNewGameIdeoOptions(HttpListenerContext context)
        {
            await context.SendJsonResponse(ApiResult<Dictionary<string, object>>.Ok(
                GamePlayHelper.GetNewGameIdeoOptions()));
        }

        [Post("/api/v1/game/start")]
        public async Task GameStart(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<NewGameStartRequestDto>();
            var result = _gameStateService.GameStart(body);
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/game/options")]
        [EndpointMetadata("Read in-game storyteller, difficulty, anomaly and gameplay options")]
        public async Task GetGameOptions(HttpListenerContext context)
        {
            await context.SendJsonResponse(_gameStateService.GetGameOptions());
        }

        [Post("/api/v1/game/options")]
        [EndpointMetadata("Apply in-game storyteller, difficulty, anomaly and gameplay options")]
        public async Task SetGameOptions(HttpListenerContext context)
        {
            var body = await context.Request.ReadBodyAsync<GameOptionsRequest>();
            await context.SendJsonResponse(_gameStateService.SetGameOptions(body));
        }

        [Get("/api/v1/game/settings")]
        public async Task GetGameSettings(HttpListenerContext context)
        {
            await _cachingService.CacheAwareResponseAsync(
                context,
                "/api/v1/game/settings",
                dataFactory: () => Task.FromResult(_gameStateService.GetCurrentSettings()),
                expiration: TimeSpan.FromSeconds(15),
                priority: CachePriority.Low,
                expirationType: CacheExpirationType.Absolute
            );
        }

        [Post("/api/v1/game/settings/toggle/run-in-background")]
        public async Task ToggleRunInBackground(HttpListenerContext context)
        {
            var result = _gameStateService.ToggleRunInBackground();
            await context.SendJsonResponse(result);
        }

        [Get("/api/v1/game/settings/run-in-background")]
        public async Task GetRunInBackground(HttpListenerContext context)
        {
            var result = _gameStateService.GetRunInBackground();
            await context.SendJsonResponse(result);
        }
    }
}
