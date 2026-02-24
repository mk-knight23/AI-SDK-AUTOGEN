using Microsoft.AspNetCore.SignalR;
using AutoGenBackend.Controllers;
using AutoGenBackend.Hubs;
using AutoGenBackend.Repositories;
using AutoGenBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AI-SDK-AUTOGEN API",
        Version = "v1",
        Description = "Multi-agent system API built with .NET 9 and AutoGen"
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:3000",
                "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Register repositories
builder.Services.AddSingleton<IRepository<Models.Agent>, InMemoryAgentRepository>();
builder.Services.AddSingleton<IRepository<Models.Conversation>, InMemoryConversationRepository>();
builder.Services.AddSingleton<IRepository<Models.CodeExecution>, InMemoryCodeExecutionRepository>();
builder.Services.AddSingleton<IRepository<Models.GroupChat>, InMemoryGroupChatRepository>();

builder.Services.AddSingleton<IAgentRepository, InMemoryAgentRepository>();
builder.Services.AddSingleton<IConversationRepository, InMemoryConversationRepository>();
builder.Services.AddSingleton<ICodeExecutionRepository, InMemoryCodeExecutionRepository>();
builder.Services.AddSingleton<IGroupChatRepository, InMemoryGroupChatRepository>();

// Register services
builder.Services.AddSingleton<ConnectionMapping>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<ConversationService>();
builder.Services.AddScoped<CodeExecutionService>();
builder.Services.AddScoped<GroupChatService>();
builder.Services.AddSingleton<AgentOrchestrationService>();

// Register Docker sandbox (use in-memory mock for development)
builder.Services.AddSingleton<IDockerSandbox, InMemoryDockerSandbox>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI-SDK-AUTOGEN API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<AgentHub>("/hubs/agent");

// Health check endpoint
app.MapHealthChecks("/health");

// API info endpoint
app.MapGet("/api", () =>
{
    return Results.Ok(new
    {
        Name = "AI-SDK-AUTOGEN API",
        Version = "1.0.0",
        Description = "Multi-agent system API built with .NET 9 and AutoGen",
        Endpoints = new
        {
            Agents = "/api/agents",
            Conversations = "/api/conversations",
            CodeExecution = "/api/code",
            Groups = "/api/groups",
            AutoGen = "/api/autogen",
            Hub = "/hubs/agent"
        }
    });
})
.WithName("GetApiInfo")
.WithOpenApi();

// Root health endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Service = "AI-SDK-AUTOGEN API",
        Version = "1.0.0"
    });
})
.WithName("Root")
.WithOpenApi();

app.Run();
