using MppTests.Models;
using System.Text.Json;

namespace MppTests.Api.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var colorDataRequest = new UserColor
            {
                Colors = new List<ColorItem> 
                {
                    new ColorItem
                    {
                        Color = "Красный",
                        Percentage = 30
                    }
                }
            };

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                //WriteIndented = false
            };
            var jsonContent = JsonSerializer.Serialize(colorDataRequest, options);
        }
    }
}
