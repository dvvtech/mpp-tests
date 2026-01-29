
using MppTests.Api.BLL.Abstract;
using MppTests.Api.BLL.Exceptions;
using MppTests.Api.Models;
using MppTests.Models;
using System.Reflection;
using System.Text.Json;

namespace MppTests.Api.BLL.Services
{
    public class ColorPsychologyService : IColorPsychologyService
    {
        private readonly IAiClient _aiClient;        

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

        public ColorPsychologyService(IAiClient aiClient)
        {
            _aiClient = aiClient;            
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

            string responseJson;
            try
            {
                responseJson = await _aiClient.GetTextResponseAsync(
                    userPrompt,
                    prompt.SystemPrompt,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceException("AI Client", $"AI service request failed", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new ExternalServiceException("AI Client", "AI service timeout", ex);
            }

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
                //_logger.LogError(ex, "LLM вернула некорректный JSON. Response: {Response}", responseJson);
                throw new LlmInvalidResponseException(responseJson, ex);
            }
        }

        public static string LoadPromptFromResources()
        {
            var assembly = Assembly.GetExecutingAssembly();

#if DEBUG
            var resourceNames = assembly.GetManifestResourceNames();
            if (!resourceNames.Contains(PromptResourceName))
            {                
                throw new ResourceNotFoundException(PromptResourceName);
            }
#endif
            using var stream = assembly.GetManifestResourceStream(PromptResourceName);
            if (stream == null)
                throw new ResourceNotFoundException(PromptResourceName);

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
