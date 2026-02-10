using RepoGuard.Core.Interfaces;
using RepoGuard.Models;
using System.Text.Json;
using System.Threading.Channels;

namespace RepoGuard;

public class WebhookProcessorWorker : BackgroundService
{
    private readonly Channel<WebhookEvent> _channel;
    private readonly IEnumerable<IAnomalyDetector> _detectors;
    private readonly INotificationService _notifier;
    private readonly ILogger<WebhookProcessorWorker> _logger;

    public WebhookProcessorWorker(
        Channel<WebhookEvent> channel,
        IEnumerable<IAnomalyDetector> detectors,
        INotificationService notifier,
        ILogger<WebhookProcessorWorker> logger)
    {
        _channel = channel;
        _detectors = detectors;
        _notifier = notifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RepoGuard Worker started processing.");

        await foreach (var item in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessEventAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event type {EventType}", item.EventType);
            }
        }
    }

    private async Task ProcessEventAsync(WebhookEvent item)
    {
        using var doc = JsonDocument.Parse(item.Payload);
        var root = doc.RootElement;

        var tasks = _detectors
            .Where(d => d.CanHandle(item.EventType))
            .Select(async detector =>
            {
                try
                {
                    var anomaly = await detector.DetectAnomalyAsync(root);
                    if (!string.IsNullOrEmpty(anomaly))
                    {
                        await _notifier.NotifyAsync($"[ALERT] {anomaly}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Detector {Detector} failed.", detector.GetType().Name);
                }
            });

        await Task.WhenAll(tasks);
    }
}