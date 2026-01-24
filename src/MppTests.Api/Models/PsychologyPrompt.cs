using MppTests.Models;
using System.Text.Json.Serialization;

namespace MppTests.Api.Models
{
    public class PsychologyPrompt
    {
        [JsonPropertyName("system_prompt")]
        public string SystemPrompt { get; set; }

        [JsonPropertyName("color_data")]
        public ColorDataRequest ColorData { get; set; }

        [JsonPropertyName("output_format")]
        public List<string> OutputFormat { get; set; } = new()
        {
            "Основная характеристика",
            "Сильные стороны",
            "Рекомендации",
            "Процентное соотношение цветов"
        };
    }
}
