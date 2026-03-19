# AGENTS.md - MppTests Codebase Guide

## Project Overview

This is a .NET 9.0 ASP.NET Core Web API solution for color psychology analysis using AI/LLM services. The solution integrates with OpenAI's ChatGPT API for psychological analysis based on user color preferences.

## Solution Structure

```
MppTests/
├── src/
│   ├── MppTests.Api/          # Main API project (controllers, services, BLL)
│   ├── MppTests.Models/       # Shared DTOs and models
│   └── MppTests.Client/       # HTTP client library
├── tests/
│   └── MppTests.Api.Tests/    # xUnit test project
└── deploy/                    # Docker deployment configs
```

## Build, Run, and Test Commands

### Build the Solution
```bash
dotnet build                           # Build entire solution
dotnet build src/MppTests.Api         # Build specific project
dotnet build -c Release               # Build in Release mode
```

### Run the Application
```bash
dotnet run --project src/MppTests.Api                    # Run API
dotnet run --project src/MppTests.Api --urls "http://localhost:5000"
```

### Run Tests
```bash
dotnet test                                          # Run all tests
dotnet test tests/MppTests.Api.Tests                # Run specific test project
dotnet test --filter "FullyQualifiedName~Test1"     # Run specific test by name
dotnet test --filter "ClassName=UnitTest1"          # Run all tests in a class
dotnet test --logger "console;verbosity=detailed"   # Verbose output
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~MppTests.Api.Tests.UnitTest1.Test1"
```

### Docker Commands
```bash
docker build -f src/MppTests.Api/Dockerfile -t mpptests .
docker-compose -f deploy/docker-compose.yml up
```

## Code Style Guidelines

### Imports Order
1. System namespaces (System.*, System.Text.*, etc.)
2. Microsoft namespaces (Microsoft.AspNetCore.*, etc.)
3. Third-party namespaces
4. Project namespaces (MppTests.*)

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MppTests.Api.BLL.Abstract;
using MppTests.Models;
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `ColorPsychologyService` |
| Interfaces | IPascalCase | `IColorPsychologyService` |
| Methods | PascalCase | `AnalyzeColorPreferencesAsync` |
| Properties | PascalCase | `UserColor` |
| Private fields | _camelCase | `_httpClient`, `_logger` |
| Constants | PascalCase or SCREAMING_SNAKE | `PromptResourceName` |
| Parameters | camelCase | `cancellationToken` |

### Async Methods
All async methods must end with `Async` suffix and accept `CancellationToken`:

```csharp
public async Task<PsychologicalAnalysisResponse> AnalyzeColorPreferencesAsync(
    ApiRequest request,
    CancellationToken cancellationToken = default)
```

### Dependency Injection
Constructor injection with null checks for required dependencies:

```csharp
private readonly IColorPsychologyService _psychologyService;
private readonly ILogger<ColorAnalysisController> _logger;

public ColorAnalysisController(
    IColorPsychologyService psychologyService,
    ILogger<ColorAnalysisController> logger)
{
    _psychologyService = psychologyService;
    _logger = logger;
}
```

### Controllers
- Inherit from `ControllerBase`
- Use `[ApiController]` and `[Route]` attributes
- Route format: `v1/{resource-name}` (kebab-case in URL)
- Return `ActionResult<T>` for flexibility
- Use `CancellationToken` parameter for long-running operations

```csharp
[Route("v1/color-analysis")]
[ApiController]
public class ColorAnalysisController : ControllerBase
{
    [HttpPost("analyze-lusher")]
    public async Task<ActionResult<PsychologicalAnalysisResponse>> AnalyzeByLusherMethod(
        [FromBody] ApiRequest request,
        CancellationToken cancellationToken = default)
}
```

### Services and BLL Organization
- Place in `BLL/Services/` directory
- Interface in `BLL/Abstract/` directory
- Use scoped lifetime for services: `_builder.Services.AddScoped<IService, Service>()`

### Exception Handling
Custom exceptions inherit from base `ColorAnalysisException`:

```csharp
public class ExternalServiceException : ColorAnalysisException
{
    public string ServiceName { get; }
    
    public ExternalServiceException(
        string serviceName,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ServiceName = serviceName;
    }
}
```

Return `ProblemDetails` for HTTP error responses:

```csharp
return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
{
    Title = "AI Service Error",
    Detail = "The AI service returned an invalid response",
    Status = StatusCodes.Status500InternalServerError,
    Extensions = { ["internalErrorCode"] = "LLM_INVALID_RESPONSE" }
});
```

### Models and DTOs
- Place shared models in `MppTests.Models` project
- Use `JsonPropertyName` for snake_case JSON properties:

```csharp
public class UserColor
{
    [JsonPropertyName("colors")]
    public List<ColorItem> Colors { get; set; }
    
    [JsonPropertyName("zodiac_sign")]
    public string ZodiacSign { get; set; }
}
```

### Configuration Classes
Use `init` for configuration properties and define a `SectionName` constant:

```csharp
public class AiClientConfig
{
    public const string SectionName = "AiClientConfig";
    public string OpenAiApiKey { get; init; }
}
```

### Formatting
- Use 4 spaces indentation (no tabs)
- Opening braces on same line
- Blank line between methods
- Nullable reference types enabled: `Nullable>enable</Nullable>`
- Implicit usings enabled: `ImplicitUsings>enable</ImplicitUsings>`

### Logging
Use structured logging with parameter placeholders:

```csharp
_logger.LogError(ex, "LLM returned invalid JSON. Raw response length: {ResponseLength}", 
    ex.RawResponse?.Length ?? 0);
_logger.LogInformation("Client cancelled the request");
```

### Static JsonSerializerOptions
Define as static readonly for reuse:

```csharp
private static readonly JsonSerializerOptions SerializerOptions = new()
{
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
```

## Architecture Patterns

- **Layered Architecture**: Controllers -> Services -> Clients
- **Dependency Injection**: All dependencies injected via constructor
- **HttpClient Factory**: Use `IHttpClientFactory` or typed clients
- **Configuration**: Options pattern with `IOptions<T>`
- **Error Handling**: Custom exceptions with specific HTTP status codes

## Environment-Specific Behavior

- **Development**: Swagger enabled, CORS allows all origins
- **Production**: Specific CORS origins, no Swagger UI

## Key Files Reference

| Purpose | Location |
|---------|----------|
| Entry point | `src/MppTests.Api/Program.cs` |
| DI configuration | `src/MppTests.Api/AppStart/Startup.cs` |
| Controllers | `src/MppTests.Api/Controllers/` |
| Services | `src/MppTests.Api/BLL/Services/` |
| Interfaces | `src/MppTests.Api/BLL/Abstract/` |
| Exceptions | `src/MppTests.Api/BLL/Exceptions/` |
| Configuration | `src/MppTests.Api/Configuration/` |
| Tests | `tests/MppTests.Api.Tests/` |
