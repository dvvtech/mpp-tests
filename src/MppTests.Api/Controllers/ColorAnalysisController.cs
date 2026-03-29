
using Microsoft.AspNetCore.Mvc;
using MppTests.Api.BLL.Abstract;
using MppTests.Api.BLL.Exceptions;
using MppTests.Api.Models;
using MppTests.Models;
using System.Text.Json;

namespace MppTests.Api.Controllers
{
    [Route("v1/color-analysis")]
    [ApiController]
    public class ColorAnalysisController : ControllerBase
    {
        private readonly IColorPsychologyService _psychologyService;
        private readonly ILogger<ColorAnalysisController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ColorAnalysisController(
            IColorPsychologyService psychologyService,
            IHttpClientFactory httpClientFactory,
            ILogger<ColorAnalysisController> logger)
        {
            _psychologyService = psychologyService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }        

        [HttpPost("analyze-lusher")]
        public async Task<ActionResult<PsychologicalAnalysisResponse>> AnalyzeByLusherMethod(
            [FromBody] ApiRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{DateTime.Now.ToString()} analize test");

            try
            {
                _ = TrackVisitMppTestsAsync();

                var analysis = await _psychologyService.AnalyzeColorPreferencesAsync(request);
                return Ok(analysis);
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogError(ex, "Required resource not found");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Configuration Error",
                    Detail = "Required system resources are missing",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["internalErrorCode"] = "RESOURCE_NOT_FOUND" }
                });
            }
            catch (LlmInvalidResponseException ex)
            {                
                _logger.LogError(ex,
                    "LLM returned invalid JSON. Raw response length: {ResponseLength}",
                    ex.RawResponse?.Length ?? 0);
             
                if (ex.RawResponse != null)
                {
                    _logger.LogDebug("Invalid LLM response: {RawResponse}", ex.RawResponse);
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "AI Service Error",
                    Detail = "The AI service returned an invalid response",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["internalErrorCode"] = "LLM_INVALID_RESPONSE" }
                });
            }
            catch (ExternalServiceException ex) when (ex.InnerException is HttpRequestException httpEx)
            {
                _logger.LogError(ex, "External service call failed");
                
                var statusCode = httpEx.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => StatusCodes.Status400BadRequest,
                    System.Net.HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
                    System.Net.HttpStatusCode.Unauthorized => StatusCodes.Status401Unauthorized,
                    System.Net.HttpStatusCode.Forbidden => StatusCodes.Status403Forbidden,
                    System.Net.HttpStatusCode.RequestTimeout => StatusCodes.Status408RequestTimeout,
                    System.Net.HttpStatusCode.GatewayTimeout => StatusCodes.Status504GatewayTimeout,
                    _ => StatusCodes.Status502BadGateway
                };

                var title = statusCode switch
                {
                    StatusCodes.Status408RequestTimeout => "AI Service Timeout",
                    StatusCodes.Status504GatewayTimeout => "AI Service Timeout",
                    _ => "AI Service Unavailable"
                };

                return StatusCode(statusCode, new ProblemDetails
                {
                    Title = title,
                    Detail = "The AI service is temporarily unavailable",
                    Status = statusCode
                });
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {                
                _logger.LogInformation("Client cancelled the request");
                return StatusCode(499); // Client Closed Request
            }
            //catch (ExternalServiceException ex) when (ex.InnerException is TaskCanceledException)
            //{
            //    _logger.LogError(ex, "AI service timeout or operation canceled");

            //    return StatusCode(StatusCodes.Status504GatewayTimeout, new ProblemDetails
            //    {
            //        Title = "AI Service Timeout or operation canceled",
            //        Detail = "The AI service did not respond in time",
            //        Status = StatusCodes.Status504GatewayTimeout
            //    });
            //}
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "External ai service error");

                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Title = "External AI Service Error",
                    Detail = "Error communicating with external service",
                    Status = StatusCodes.Status502BadGateway
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "JSON serialization error");

                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Data Format",
                    Detail = "The provided data contains invalid format",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request was cancelled by client");
                return StatusCode(499); // Client Closed Request
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during color analysis");

                // Для production - общее сообщение
                var detail = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                    ? ex.Message
                    : "An unexpected error occurred";

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = detail,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("analyze-lusher-test")]
        public async Task<ActionResult<PsychologicalAnalysisResponse>> AnalyzeByLusherMethodTest(
            [FromBody] ApiRequest request,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(4000);

            return Ok(new PsychologicalAnalysisResponse
            {
                MainCharacteristic = "Вас характеризует стремление к гармонии и стабильности, что проявляется в значительном предпочтении зеленого цвета. Это указывает на вашу потребность в признании и защите собственных границ. Оранжевый цвет подчеркивает вашу коммуникабельность и оптимизм в общении с окружающими, несмотря на возраст зрелости, когда часто происходит переоценка ценностей. Как Водолей, вы отличаетесь гибкостью и интеллектом, что помогает вам находить баланс между внутренним миром и социальной активностью.",
                Strengths = new List<string>
                {
                    "Гармония и стабильность",
                    "Коммуникабельность",
                    "Интеллект и гибкость",
                    "Гармония"
                },
                Recommendations = new List<string>
                {
                    "Сохраняйте баланс между внутренней гармонией и социальной активностью.",
                    "Используйте свою коммуникабельность для установления полезных связей.",
                    "Не забывайте о необходимости личного пространства, чтобы поддерживать внутреннюю стабильность.",
                    "Продолжайте развивать свои интеллектуальные способности и гибкость мышления."
                }
            });
        }

        private async Task TrackVisitMppTestsAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();

            var clientIp = GetRealClientIp(HttpContext);

            // Создаем запрос к analytics
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://analytics_api:8080/v1/analytics/track-mpptests");

            request.Headers.Add("X-Forwarded-For", clientIp);
            request.Headers.Add("X-Real-IP", clientIp);
            request.Headers.Add("X-Operation-Type", "calc");

            // Прокидываем оригинальный User-Agent
            var userAgent = Request.Headers["User-Agent"].ToString();
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.Headers.Add("User-Agent", userAgent);
            }

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Analytics tracking failed: {response.StatusCode}");
            }
        }

        private string GetRealClientIp(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Берем первый IP из цепочки (реальный клиентский)
                return forwardedFor.Split(',').First().Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Если нет заголовков, используем RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
