
using System.Text.Json.Serialization;

namespace MppTests.Models
{
    public class ColorDataRequest
    {
        [JsonPropertyName("colors")]
        public List<ColorItem> Colors { get; set; }
    }

    public class ColorItem
    {
        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("percentage")]
        public double Percentage { get; set; }
    }
}
