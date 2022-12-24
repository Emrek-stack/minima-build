using Newtonsoft.Json;

namespace Minima.Build.Utils
{
    public class NukeParameters
    {
        [JsonProperty("$schema")]
        public string Schema { get; set; } = "./build.schema.json";
        public string Solution { get; set; }
    }
}
