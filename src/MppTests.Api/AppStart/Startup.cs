using MppTests.Api.AppStart.Extensions;
using MppTests.Api.BLL.Abstract;
using MppTests.Api.BLL.Services;
using MppTests.Api.Configuration;
using System.Net;
using System.Threading.RateLimiting;

namespace MppTests.Api.AppStart
{
    public class Startup
    {
        private WebApplicationBuilder _builder;

        public Startup(WebApplicationBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public void Initialize()
        {
            if (_builder.Environment.IsDevelopment())
            {
                _builder.Services.AddSwaggerGen();
            }
            else
            {
                _builder.Services.ConfigureCors();
            }

            // Регистрация HttpClientFactory
            _builder.Services.AddHttpClient();

            InitConfigs();
            ConfigureServices();
            ConfigureClientAPI();
            ConfigureRateLimiting();

            _builder.Services.AddControllers();
        }

        private void InitConfigs()
        {
            if (!_builder.Environment.IsDevelopment())
            {
                _builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);
            }

            _builder.Services.Configure<AiClientConfig>(_builder.Configuration.GetSection(AiClientConfig.SectionName));
            _builder.Services.Configure<ProxyConfig>(_builder.Configuration.GetSection(ProxyConfig.SectionName));

            //var logger = _builder.Services.BuildServiceProvider().GetService<ILogger<Startup>>();
            //var smtpConfig = _builder.Configuration.GetSection(AiClientConfig.SectionName).Get<AiClientConfig>();
            //logger.LogInformation("key:" + smtpConfig.OpenAiApiKey);                        
        }

        private void ConfigureServices()
        {
            _builder.Services.AddScoped<IAnalyticsTrackingService, AnalyticsTrackingService>();
            _builder.Services.AddScoped<IPromptService, PromptService>();
            _builder.Services.AddScoped<IColorPsychologyService, ColorPsychologyService>();
        }

        private void ConfigureRateLimiting()
        {
            _builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Success = false,
                        ErrorMessage = "Превышено количество запросов. Попробуйте позже."
                    }, cancellationToken);
                };

                options.AddPolicy("MppRequests", httpContext =>
                {
                    var clientIp = httpContext.GetRealClientIp();

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientIp,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                });
            });
        }

        private void ConfigureClientAPI()
        {                        
            _builder.Services.AddHttpClient<IAiClient, ChatGptAiClient>((serviceProvider, client) =>
            {
                var aiClientConfig = _builder.Configuration.GetSection(AiClientConfig.SectionName).Get<AiClientConfig>();

                client.BaseAddress = new Uri("https://api.openai.com/v1/chat/completions");
                client.Timeout = TimeSpan.FromSeconds(35); // Таймаут запроса
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {aiClientConfig.OpenAiApiKey}");
            })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();

                    // Получаем настройки прокси из конфигурации
                    var proxyConfig = _builder.Configuration.GetSection("ProxySettings").Get<ProxyConfig>();

                    if (proxyConfig?.Enabled == true && !string.IsNullOrEmpty(proxyConfig.Ip))
                    {
                        var proxy = new WebProxy
                        {
                            Address = new Uri($"http://{proxyConfig.Ip}:{proxyConfig.Port}"),
                            BypassProxyOnLocal = false,
                            UseDefaultCredentials = false
                        };

                        // Если есть логин и пароль
                        if (!string.IsNullOrEmpty(proxyConfig.Login) && !string.IsNullOrEmpty(proxyConfig.Password))
                        {
                            proxy.Credentials = new NetworkCredential(proxyConfig.Login, proxyConfig.Password);
                        }

                        handler.Proxy = proxy;
                        handler.UseProxy = true;
                    }
                    return handler;
                });
        }
    }
}
