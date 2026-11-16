namespace RIMAPI.Models
{
    public class GameLoadRequestDto
    {
        public string FileName { get; set; }
        public bool CheckVersion { get; set; }
        public bool SkipModMismatch { get; set; }
    }

    public class GameSaveRequestDto
    {
        public string FileName { get; set; }
        /// <summary>Allow replacing an existing non-commitment save.</summary>
        public bool Overwrite { get; set; }
    }
}
