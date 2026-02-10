using Microsoft.AspNetCore.Mvc;
using RepoGuard;
using RepoGuard.Core.Constants;
using RepoGuard.Core.Interfaces;
using RepoGuard.Core.Security;
using RepoGuard.Logic.DI;
using RepoGuard.Models;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddSingleton(Channel.CreateBounded<WebhookEvent>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
}));

// Handles the queue processing in the background. This allows us to respond immediately to GitHub and easily add more consumers later for high-load scenarios.
builder.Services.AddHostedService<WebhookProcessorWorker>();
builder.Services.AddSingleton<ISignatureValidator, SignatureValidator>();


builder.Services
    .AddNotificationService()
    .AddAnomalyDetectors()
    ;



var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal Server Error");
    });
});

app.MapPost("/webhook", async (
    HttpContext context,
    IConfiguration config,
    ISignatureValidator signatureValidator,
    Channel <WebhookEvent> channel,
    [FromHeader(Name = GithubConstants.Headers.Event)] string eventType,
    [FromHeader(Name = GithubConstants.Headers.Signature)] string? signature) =>
{
    var secret = config["Github:WebhookSecret"];
    var isValidationDisabled = string.IsNullOrEmpty(secret);

    using var reader = new StreamReader(context.Request.Body);
    var payload = await reader.ReadToEndAsync();

    if (!isValidationDisabled && !signatureValidator.IsValid(payload, signature, secret!))
    {
        Console.WriteLine($"[SECURITY] Invalid signature from IP: {context.Connection.RemoteIpAddress}");
        return Results.BadRequest("Invalid Signature");
    }

    await channel.Writer.WriteAsync(new WebhookEvent(eventType, payload));

    return Results.Accepted();
});

Console.WriteLine("=====================================================");
Console.WriteLine("      RepoGuard - Anomaly Detection Started          ");
Console.WriteLine("   You may set Git Server to send events to /webhook  ");
Console.WriteLine("=====================================================");

app.Run();
