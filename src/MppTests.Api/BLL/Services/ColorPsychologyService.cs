
using MppTests.Api.BLL.Abstract;
using MppTests.Api.BLL.Exceptions;
using MppTests.Api.Models;
using MppTests.Models;
using System.Text.Json;

namespace MppTests.Api.BLL.Services
{
    public class ColorPsychologyService : IColorPsychologyService
    {
        private readonly IAiClient _aiClient;
        private readonly IPromptService _promptService;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly JsonSerializerOptions DeserializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ColorPsychologyService(IAiClient aiClient, IPromptService promptService)
        {
            _aiClient = aiClient;
            _promptService = promptService;
        }

        public async Task<PsychologicalAnalysisResponse> AnalyzeColorPreferencesAsync(
            ApiRequest request,
            CancellationToken cancellationToken = default)
        {
            var prompt = new PsychologyPrompt()
            {
                UserColor = request.UserColor
            };

            prompt.SystemPrompt = _promptService.GetSystemPrompt(request.Version);
            prompt.UserPrompt = _promptService.GetUserPrompt(request.Version);

            return await SendToLlmAsync(prompt);                      
        }

        private async Task<PsychologicalAnalysisResponse> SendToLlmAsync(
            PsychologyPrompt prompt,
            CancellationToken cancellationToken = default)
        {                        
            var userColorJson = JsonSerializer.Serialize(prompt.UserColor, SerializerOptions);
            var userPrompt = string.Format(prompt.UserPrompt, userColorJson);

            string responseJson = string.Empty;
            try
            {
                responseJson = await _aiClient.GetTextResponseAsync(
                    userPrompt,
                    prompt.SystemPrompt,
                    cancellationToken);

                return JsonSerializer.Deserialize<PsychologicalAnalysisResponse>(
                    responseJson,
                    DeserializerOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceException("AI Client", $"AI service request failed", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {             
                throw new ExternalServiceException("AI Client", "AI service timeout", ex);
            }
            catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                // Если клиент отменил - пробрасываем как есть
                throw;
            }
            catch (JsonException ex)
            {                
                throw new LlmInvalidResponseException(responseJson, ex);
            }            
        }        
    }
}
