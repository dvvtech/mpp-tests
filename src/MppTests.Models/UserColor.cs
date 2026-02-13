
using System.Text.Json.Serialization;

namespace MppTests.Models
{
    public class ApiRequest
    {
        [JsonPropertyName("userData")]
        public UserColor UserData { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }
    }

    public class UserColor
    {
        [JsonPropertyName("colors")]
        public List<ColorItem> Colors { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("zodiacSign")]
        public string ZodiacSign { get; set; }
    }

    public class ColorItem
    {
        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("percentage")]
        public double Percentage { get; set; }
    }
}
