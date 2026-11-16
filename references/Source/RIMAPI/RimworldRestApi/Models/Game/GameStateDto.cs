using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class GameStateDto
    {
        public int GameTick { get; set; }
        public float ColonyWealth { get; set; }
        public int ColonistCount { get; set; }
        public string Storyteller { get; set; }
        public bool IsPaused { get; set; }
        public string ProgramState { get; set; }
        public string CurrentView { get; set; }
        public bool IsSettingsOpen { get; set; }
        public bool IsModSettingsOpen { get; set; }
        public int MapCount { get; set; }
    }

    public class NewGameStartRequestDto
    {
        public string StorytellerName { get; set; }
        public string DifficultyName { get; set; }
        public int MapSize { get; set; }
        public bool Permadeath { get; set; }
        public float PlanetCoverage { get; set; }
        public string WorldSeed { get; set; }
        public string StartingTile { get; set; }
        public string StartingSeason { get; set; }
        public int OverallRainfall { get; set; }
        public int OverallTemperature { get; set; }
        public int OverallPopulation { get; set; }
        public int LandmarkDensity { get; set; }
        /// <summary>Optional initial-game ideology selection/configuration (C-04).</summary>
        public NewGameIdeoDto Ideo { get; set; }
        /// <summary>Optional scenario selection/editor (C-01).</summary>
        public NewGameScenarioDto Scenario { get; set; }
        /// <summary>Optional explicit world parameters (C-02).</summary>
        public NewGameWorldDto World { get; set; }
        /// <summary>Optional starting-site/map/season selection (C-02).</summary>
        public NewGameSiteDto Site { get; set; }
        /// <summary>Optional starting-pawn reroll/xenotype/keep instructions (C-03).</summary>
        public NewGameStartingPawnsDto StartingPawns { get; set; }
        /// <summary>Explicit acknowledgement for winter/large-map warnings (C-02).</summary>
        public bool ConfirmWarnings { get; set; }
        /// <summary>Entry-equivalent native flag; lets scenario PostGameStart run (C-39).</summary>
        public bool StartedFromEntry { get; set; }
        /// <summary>Prepare scenario/world/ideology/starting pawns without generating the map (C-03).</summary>
        public bool PrepareOnly { get; set; }
    }

    public class NewGameIdeoDto
    {
        /// <summary>classic, preset, fixed, fluid, or load (a saved .rid).</summary>
        public string Mode { get; set; }
        public string PresetName { get; set; }
        /// <summary>Listed saved .rid file name, only for mode=load; no paths.</summary>
        public string PresetFile { get; set; }
        /// <summary>Acknowledge the native saved-file version/mod warnings.</summary>
        public bool ConfirmWarnings { get; set; }
        public string Name { get; set; }
        public string StructureMeme { get; set; }
        public List<string> Memes { get; set; }
        public List<string> Styles { get; set; }
    }

    public class GameSettingsDto
    {
        // --- General / System ---
        public string Language { get; set; }
        public bool RunInBackground { get; set; }
        public bool DevelopmentMode { get; set; }
        public bool LogVerbose { get; set; }        // If verbose logging is enabled
        public string TemperatureMode { get; set; } // "Celsius", "Fahrenheit", "Kelvin"
        public bool AutosaveInterval { get; set; }  // Usually in days (float or int representation)

        // --- Graphics / Display ---
        public string Resolution { get; set; }      // Format: "1920x1080"
        public bool Fullscreen { get; set; }
        public float UserInterfaceScale { get; set; } // Changed to float (e.g., 1.0, 1.25)
        public bool CustomCursorEnabled { get; set; }
        public bool HatsOnlyOnMap { get; set; }     // "Show hats only on map" setting
        public bool PlantWindSway { get; set; }
        public int MaxNumberOfPlayerSettlements { get; set; } // Default is 1, up to 5

        // --- Audio (Volumes 0.0 to 1.0) ---
        public float VolumeMaster { get; set; }
        public float VolumeGame { get; set; }
        public float VolumeMusic { get; set; }
        public float VolumeAmbient { get; set; }

        // --- Gameplay / Controls ---
        public bool PauseOnLoad { get; set; }
        public string PauseOnUrgentLetter { get; set; }
        public bool AutomaticPauseOnLetter { get; set; } // Any letter? (Check nuances)
        public bool EdgeScreenScroll { get; set; }
        public float MapDragSensitivity { get; set; }
        public bool ZoomToMouse { get; set; }

        // --- Interface / HUD ---
        public bool ShowRealtimeClock { get; set; }
        public bool ResourceReadoutCategorized { get; set; } // The tree view on the left
        public bool ShowAnimalNames { get; set; }

        // --- Modding (Optional) ---
        public bool ResetModsConfigOnCrash { get; set; }
        public int TextureCompression { get; set; } // Often hidden but accessible
    }
}

// AI-FORK: C-05 in-game storyteller/difficulty/anomaly/gameplay options.
public sealed class GameOptionsRequest
{
    public string Storyteller { get; set; }
    public string Difficulty { get; set; }
    /// <summary>Difficulty field map (snake_case) applied on top of the selected difficulty.</summary>
    public Dictionary<string, object> CustomDifficulty { get; set; }
    public GameAnomalyOptions Anomaly { get; set; }
    public int? MaxNumberOfPlayerSettlements { get; set; }
    public List<string> PreferredNames { get; set; }
    public bool DryRun { get; set; }
}

public sealed class GameAnomalyOptions
{
    public string Playstyle { get; set; }
    public float? ThreatsInactive { get; set; }
    public float? ThreatsActive { get; set; }
    public float? OverrideThreat { get; set; }
    public float? StudyEfficiency { get; set; }
}
