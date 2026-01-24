using System.Text.Json.Serialization;

namespace MppTests.Api.Models
{
    public class PsychologicalAnalysisResponse
    {
        [JsonPropertyName("main_characteristic")]
        public string MainCharacteristic { get; set; }

        [JsonPropertyName("strengths")]
        public List<string> Strengths { get; set; } = new();

        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; set; } = new();
    }
}
