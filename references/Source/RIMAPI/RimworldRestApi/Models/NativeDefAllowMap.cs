using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RIMAPI.Models
{
    // Dictionary keys are native DefNames, not JSON field names. Preserve them
    // so a complete readback can be sent back as a filter edit.
    public static class NativeDefAllowMap
    {
        public static JObject Create(IEnumerable<KeyValuePair<string, bool>> entries)
        {
            var result = new JObject();
            foreach (var entry in entries) result.Add(entry.Key, new JValue(entry.Value));
            return result;
        }
    }
}
