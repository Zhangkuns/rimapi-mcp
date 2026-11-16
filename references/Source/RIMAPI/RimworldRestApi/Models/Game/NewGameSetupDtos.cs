using System.Collections.Generic;

namespace RIMAPI.Models
{
    // C-01/C-02/C-03 new-game setup request objects. Existing top-level fields on
    // NewGameStartRequestDto stay valid for backward compatibility.
    public sealed class NewGameScenarioDto
    {
        public string DefName { get; set; }
        public string PresetFile { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public List<NewGameScenarioPartDto> Parts { get; set; }
        public string SaveAs { get; set; }
    }

    public sealed class NewGameScenarioPartDto
    {
        public string DefName { get; set; }
        // true (default) adds one randomized part; false removes matching parts.
        public bool? Enabled { get; set; }
    }

    public sealed class NewGameWorldDto
    {
        public string Seed { get; set; }
        public float? PlanetCoverage { get; set; }
        public int? OverallRainfall { get; set; }
        public int? OverallTemperature { get; set; }
        public int? OverallPopulation { get; set; }
        public int? LandmarkDensity { get; set; }
        public float? Pollution { get; set; }
        public List<NewGameFactionCountDto> Factions { get; set; }
    }

    public sealed class NewGameFactionCountDto
    {
        public string DefName { get; set; }
        public int Count { get; set; }
    }

    public sealed class NewGameSiteDto
    {
        // Numeric tile id or "auto".
        public string Tile { get; set; }
        public int? MapSize { get; set; }
        public string Season { get; set; }
    }

    public sealed class NewGameStartingPawnsDto
    {
        // Reorder these selected/optional slots to the front (normal page drag).
        public List<int> KeepIndices { get; set; }
        // Native StartingPawnUtility.RandomizePawn for these slots.
        public List<int> RerollIndices { get; set; }
        // Native forced XenotypeDef/CustomXenotype generation requests.
        public List<NewGamePawnXenotypeDto> Xenotypes { get; set; }
    }

    public sealed class NewGamePawnXenotypeDto
    {
        public int Index { get; set; }
        public string DefName { get; set; }
        public NewGameCustomXenotypeDto Custom { get; set; }
    }

    public sealed class NewGameCustomXenotypeDto
    {
        public string Name { get; set; }
        public List<string> Genes { get; set; }
        public bool Inheritable { get; set; }
        public string IconDef { get; set; }
    }

    public sealed class NewGameWorldPreviewRequestDto
    {
        public NewGameScenarioDto Scenario { get; set; }
        public NewGameWorldDto World { get; set; }
        public NewGameTileQueryDto TileQuery { get; set; }
    }

    public sealed class NewGameTileQueryDto
    {
        public string Tile { get; set; }
        public string Biome { get; set; }
        public string Hilliness { get; set; }
        public string Landmark { get; set; }
        public int? MaxResults { get; set; }
    }
}
