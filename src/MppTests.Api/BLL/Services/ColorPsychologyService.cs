
using MppTests.Api.BLL.Abstract;
using MppTests.Api.Models;
using MppTests.Models;
using System.Text.Json;

namespace MppTests.Api.BLL.Services
{
    public class ColorPsychologyService : IColorPsychologyService
    {
        private readonly IAiClient _aiClient;
        private readonly ILogger<ColorPsychologyService> _logger;

        public ColorPsychologyService(IAiClient aiClient, ILogger<ColorPsychologyService> logger)
        {
            _aiClient = aiClient;
            _logger = logger;
        }

        public async Task<PsychologicalAnalysisResponse> AnalyzeColorPreferencesAsync(ColorDataRequest request)
        {
            var prompt = new PsychologyPrompt
            {
                ColorData = request,
                SystemPrompt = @"Ты — опытный психолог, специализирующийся на проективной диагностике и анализе цветовых предпочтений по адаптированной методике Люшера. 
                          Твоя задача — проанализировать раскраску пользователя и дать развёрнутую психологическую интерпретацию.
                          
                          МЕТОДИКА АНАЛИЗА:
                          1. ПРЕОБЛАДАНИЕ КРАСНОГО (>30%): Энергичность, активность, лидерские качества
                          2. ПРЕОБЛАДАНИЕ СИНЕГО (>25%): Спокойствие, рассудительность, стабильность
                          3. ПРЕОБЛАДАНИЕ ЗЕЛЕНОГО (>20%): Баланс, гармония, естественность
                          4. ПРЕОБЛАДАНИЕ ЖЕЛТОГО (>15%): Оптимизм, креативность, общительность
                          5. ПРЕОБЛАДАНИЕ ФИОЛЕТОВОГО (>10%): Интуиция, духовность, чувствительность
                          6. ПРЕОБЛАДАНИЕ ЧЕРНОГО (>5%): Сдержанность, таинственность, депрессивные тенденции
                          7. РАВНОМЕРНОЕ РАСПРЕДЕЛЕНИЕ (все цвета 10-20%): Гармоничная личность, адаптивность
                          
                          ДОПОЛНИТЕЛЬНЫЕ ПРАВИЛА:
                          - Если один цвет >50% - указывать на возможную одержимость/фиксацию
                          - Если использовано менее 3 цветов - отметить ограниченность восприятия
                          - Сочетание теплых цветов (красный, желтый) - экстраверсия
                          - Сочетание холодных (синий, зеленый) - интроверсия
                          
                          ФОРМАТ ОТВЕТА:
                          1. Основная характеристика
                          2. Сильные стороны
                          3. Рекомендации"
            };

            var analysis = await SendToLlmAsync(prompt);

            return analysis;            
        }

        private async Task<PsychologicalAnalysisResponse> SendToLlmAsync(PsychologyPrompt prompt)
        {
            // Здесь код отправки в выбранную LLM API
            // Например: OpenAI GPT, Yandex GPT, etc.

            //var jsonContent = JsonSerializer.Serialize(prompt);
            //var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var jsonContent = JsonSerializer.Serialize(prompt.ColorData);
            //var response = await _httpClient.PostAsync("llm-api-endpoint", content);
            var responseJson = await _aiClient.GetTextResponseAsync(prompt.SystemPrompt, jsonContent);
            //response.EnsureSuccessStatusCode();

            //var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PsychologicalAnalysisResponse>(responseJson);
        }
    }
}
