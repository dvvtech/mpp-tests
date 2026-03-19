using Microsoft.Extensions.Configuration;
using MppTests.Api.BLL.Abstract;
using MppTests.Api.BLL.Services;
using MppTests.Models;
using System.Net;
using System.Text.Json;

namespace MppTests.Api.Tests
{
    public class UnitTest1
    {
        private readonly IConfiguration _configuration;

        public UnitTest1()
        {
            var apiProjectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\..\\src\\MppTests.Api"));

            _configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddUserSecrets("7c85570f-7fc9-493a-88be-a40f3645849d")
                .Build();
        }

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
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var jsonContent = JsonSerializer.Serialize(colorDataRequest, options);
        }

        [Fact]
        public async Task AnalyzeColorPreferencesAsync_ShouldReturnAnalysis()
        {
            var apiKey = _configuration["AiClientConfig:OpenAiApiKey"];
            Assert.False(string.IsNullOrEmpty(apiKey), "OpenAiApiKey not found in user secrets");

            var proxyEnabled = bool.Parse(_configuration["ProxySettings:Enabled"] ?? "false");
            var handler = new HttpClientHandler();

            if (proxyEnabled)
            {
                var proxyIp = _configuration["ProxySettings:Ip"];
                var proxyPort = _configuration["ProxySettings:Port"];
                var proxyLogin = _configuration["ProxySettings:Login"];
                var proxyPassword = _configuration["ProxySettings:Password"];

                var proxy = new WebProxy
                {
                    Address = new Uri($"http://{proxyIp}:{proxyPort}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false
                };

                if (!string.IsNullOrEmpty(proxyLogin) && !string.IsNullOrEmpty(proxyPassword))
                {
                    proxy.Credentials = new NetworkCredential(proxyLogin, proxyPassword);
                }

                handler.Proxy = proxy;
                handler.UseProxy = true;
            }

            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri("https://api.openai.com/v1/chat/completions");
            httpClient.Timeout = TimeSpan.FromSeconds(35);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var aiClient = new ChatGptAiClient(httpClient);
            var promptService = new PromptService();
            var colorPsychologyService = new ColorPsychologyService(aiClient, promptService);

            var request = new ApiRequest
            {
                UserColor = new UserColor
                {
                    Colors = new List<ColorItem>
                    {
                        new ColorItem { Color = "Зеленый", Percentage = 40 },
                        new ColorItem { Color = "Оранжевый", Percentage = 30 },
                        new ColorItem { Color = "Синий", Percentage = 20 },
                        new ColorItem { Color = "Красный", Percentage = 10 }
                    },
                    Age = 35,
                    Gender = "Мужской",
                    ZodiacSign = "Водолей"
                },
                Version = 1
            };

            var result = await colorPsychologyService.AnalyzeColorPreferencesAsync(request);

            Assert.NotNull(result);
            Assert.NotNull(result.MainCharacteristic);
        }
    }
}
