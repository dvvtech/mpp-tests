using MppTests.Api.AppStart;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder);
startup.Initialize();

var app = builder.Build();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
app.Logger.LogInformation(environment ?? "Empty environment");
// Configure the HTTP request pipeline.

var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
foreach (var name in resourceNames)
{
    //MppTests.Api.BLL.Prompts.ColorPsychologyPrompt.txt
    //_logger.LogInformation("Found resource: {ResourceName}", name);
}

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
