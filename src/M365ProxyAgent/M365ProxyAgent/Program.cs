using Azure.Monitor.OpenTelemetry.AspNetCore;
using M365ProxyAgent.Agents;
using M365ProxyAgent.Auth;
using M365ProxyAgent.Configuration;
using M365ProxyAgent.Extensions;
using M365ProxyAgent.Handlers;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.Compat;
using Microsoft.Agents.CopilotStudio.Client;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;
using Microsoft.Agents.Storage.Transcript;

var builder = WebApplication.CreateBuilder(args);

// Add configuration validation services early
builder.Services.AddConfigurationValidation();

// Add exception handling services
builder.Services.AddExceptionHandling();

var copilotSettings = new CopilotStudioClientSettings(builder.Configuration.GetSection("CopilotStudioClientSettings"));

// Add OpenTelemetry and use Azure Monitor
builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = builder.Configuration["connectionString"];
});

builder.Services.AddSingleton<Microsoft.Agents.Builder.IMiddleware[]>([new TranscriptLoggerMiddleware(new FileTranscriptLogger())]);

builder.Services.AddHttpClient("mcs").ConfigurePrimaryHttpMessageHandler(() =>
{
    return new AddTokenHandlerS2S(copilotSettings);
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Logging.AddConsole();

// Register IStorage.  For development, MemoryStorage is suitable.
// For production Agents, persisted storage should be used so
// that state survives Agent restarts, and operate correctly
// in a cluster of Agent instances.
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Add AgentApplicationOptions from config.
builder.AddAgentApplicationOptions();

// Add AgentApplicationOptions.  This will use DI'd services and IConfiguration for construction.
builder.Services.AddTransient<AgentApplicationOptions>();

// Add the bot (which is transient)
builder.AddAgent<ProxyAgent>();

builder.Services
    .AddSingleton(copilotSettings)
    .AddTransient<CopilotClient>((s) =>
    {
        var logger = s.GetRequiredService<ILoggerFactory>().CreateLogger<CopilotClient>();
        return new CopilotClient(copilotSettings, s.GetRequiredService<IHttpClientFactory>(), logger, "mcs");
    })
    // Register SOLID-compliant services
    .AddScoped<IConversationService, ConversationService>()
    .AddScoped<IMessageHandlerFactory, MessageHandlerFactory>()
    .AddScoped<WelcomeMessageHandler>()
    .AddScoped<RegularMessageHandler>()
    .AddScoped<ICorrelationService, CorrelationService>()
    .AddSingleton<IResilienceService, ResilienceService>();

var app = builder.Build();

app.Services.ValidateStartupConfiguration(builder.Configuration);

app.UseExceptionHandling();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "M365 Agent Proxy");
app.MapPost("/api/messages", async (HttpRequest request, HttpResponse response, IAgentHttpAdapter adapter, IAgent agent, CancellationToken cancellationToken) =>
{
    await adapter.ProcessAsync(request, response, agent, cancellationToken);
});

app.Run();