// AI-FORK: DTOs for the read-only /api/v1/defs family.
using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class DefTypeListDto
    {
        public int Total { get; set; }
        public List<DefTypeInfoDto> Types { get; set; }
    }

    public class DefTypeInfoDto
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public string Dlc { get; set; }
        public bool Active { get; set; }
    }

    public class DefListDto
    {
        public string Type { get; set; }
        public int Total { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public List<Dictionary<string, object>> Items { get; set; }
    }

    public class DefDetailDto
    {
        public Dictionary<string, object> Def { get; set; }
    }
}
