
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
            var domainEnergy = GetDominantEnergy(request);

            return await SendToLlmAsync(prompt, domainEnergy);                      
        }

        private async Task<PsychologicalAnalysisResponse> SendToLlmAsync(
            PsychologyPrompt prompt,
            string domainEnergy,
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

                var analysis = JsonSerializer.Deserialize<PsychologicalAnalysisDto>(
                    responseJson,
                    DeserializerOptions);

                return new PsychologicalAnalysisResponse
                {
                    MainCharacteristic = analysis.MainCharacteristic,
                    Strengths = analysis.Strengths,
                    Recommendations = analysis.Recommendations,
                    DominantEnergy = domainEnergy
                };
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

        public static string GetDominantEnergy(ApiRequest request)
        {
            if (request?.UserColor?.Colors == null || !request.UserColor.Colors.Any())
                return "Нет данных";

            var warmColors = new HashSet<string>
            {
                "Красный",
                "Оранжевый",
                "Желтый",
                "Розовый",
                "Коричневый"
            };

            var coldColors = new HashSet<string>
            {
                "Зеленый",
                "Голубой",
                "Синий",
                "Фиолетовый",
                "Бирюзовый"
            };

            double warmSum = 0;
            double coldSum = 0;

            foreach (var color in request.UserColor.Colors)
            {
                if (color?.Color == null)
                    continue;

                if (warmColors.Contains(color.Color))
                {
                    warmSum += color.Percentage;
                }
                else if (coldColors.Contains(color.Color))
                {
                    coldSum += color.Percentage;
                }
                // нейтральные (черный, белый) игнорируем
            }

            if (warmSum < 0.001 && coldSum < 0.001)
                return "Отсутствуют теплые и холодные цвета";

            if (warmSum > coldSum)
                return $"Преобладает янская энергия ({warmSum:F1}% vs {coldSum:F1}%)";

            if (coldSum > warmSum)
                return $"Преобладает иньская энергия ({coldSum:F1}% vs {warmSum:F1}%)";

            if (Math.Abs(warmSum - coldSum) < 0.001)
                return "Баланс энергий";

            return string.Empty;
        }
    }
}
