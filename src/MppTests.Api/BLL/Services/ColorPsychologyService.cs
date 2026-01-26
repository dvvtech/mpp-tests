
using MppTests.Api.BLL.Abstract;
using MppTests.Api.Models;
using MppTests.Models;
using System.Reflection;
using System.Text.Json;

namespace MppTests.Api.BLL.Services
{
    public class ColorPsychologyService : IColorPsychologyService
    {
        private readonly IAiClient _aiClient;
        private readonly ILogger<ColorPsychologyService> _logger;

        private const string PromptResourceName = "MppTests.Api.BLL.Prompts.ColorPsychologyPrompt.txt";

        private static readonly Lazy<string> SystemPromptLazy = new Lazy<string>(() =>
            LoadPromptFromResources());

        private static string SystemPrompt => SystemPromptLazy.Value;

        private const string UserPromptTemplate = """
Ниже приведены данные о цветовых предпочтениях пользователя.
Проценты показывают долю каждого цвета и суммарно составляют ~100%.

Цвета могут указаны на русском языке.

ДАННЫЕ:
{0}
""";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly JsonSerializerOptions DeserializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ColorPsychologyService(IAiClient aiClient, ILogger<ColorPsychologyService> logger)
        {
            _aiClient = aiClient;
            _logger = logger;
        }

        public async Task<PsychologicalAnalysisResponse> AnalyzeColorPreferencesAsync(
            ColorDataRequest request,
            CancellationToken cancellationToken = default)
        {
            var prompt = new PsychologyPrompt
            {
                ColorData = request,
                SystemPrompt = SystemPrompt
            };

            return await SendToLlmAsync(prompt);                      
        }

        private async Task<PsychologicalAnalysisResponse> SendToLlmAsync(
            PsychologyPrompt prompt,
            CancellationToken cancellationToken = default)
        {                        
            var colorsJson = JsonSerializer.Serialize(prompt.ColorData, SerializerOptions);
            var userPrompt = string.Format(UserPromptTemplate, colorsJson);

            var responseJson = await _aiClient.GetTextResponseAsync(
                userPrompt,
                prompt.SystemPrompt,
                cancellationToken);

            try
            {
                return JsonSerializer.Deserialize<PsychologicalAnalysisResponse>(
                    responseJson,
                    DeserializerOptions
                );
            }
            catch (JsonException ex)
            {
                //более правильно наверху залогировать
                _logger.LogError(ex, "LLM вернула некорректный JSON. Response: {Response}", responseJson);
                throw new InvalidOperationException("LLM вернула некорректный JSON", ex);
            }
        }

        public static string LoadPromptFromResources()
        {
            var assembly = Assembly.GetExecutingAssembly();

#if DEBUG
            var resourceNames = assembly.GetManifestResourceNames();
            if (!resourceNames.Contains(PromptResourceName))
            {
                var available = string.Join(", ", resourceNames);
                throw new InvalidOperationException(
                    $"Resource '{PromptResourceName}' not found. Available: {available}");
            }
#endif
            using var stream = assembly.GetManifestResourceStream(PromptResourceName);
            if (stream == null)
                throw new InvalidOperationException($"Resource '{PromptResourceName}' not found");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
