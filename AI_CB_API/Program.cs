using AI_CB_API.Controllers;
using Betalgo.Ranul.OpenAI.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5010, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1; // Force HTTP/1.1
        listenOptions.UseHttps(); // Keep HTTPS if you want
    });
});

// Add services to the container.
builder.Services.AddSignalR();

// Allow CORS for the client during development so browser preflight (OPTIONS)
// requests are accepted. In production, restrict origins as appropriate.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Register shared AI service used by controllers
builder.Services.AddSingleton<AI_CB_API.Services.IAIService, AI_CB_API.Services.AIService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenAIService(options =>
{
    options.ApiKey = builder.Configuration["OpenAI:ApiKey"] ?? "";
});

// Use default HttpClientFactory in controller; no typed client required here.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Log incoming requests and responses to help debug routing/404s
app.Use(async (context, next) =>
{
    Console.WriteLine($"Incoming: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Outgoing: {context.Response.StatusCode}");
});

app.UseHttpsRedirection();

// Enable CORS policy so preflight OPTIONS requests are handled
app.UseCors("AllowDev");

app.UseAuthorization();

app.MapControllers();

app.Run();
