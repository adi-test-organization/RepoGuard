using System.Text.Json;

namespace RepoGuard.Core.Interfaces;

public interface IAnomalyDetector
{
    /// <summary>
    /// Determines if this detector should run for the specific event type.
    /// </summary>
    bool CanHandle(string eventType);

    /// <summary>
    /// Analyzes the payload and returns an alert message if suspicious, or null if safe.
    /// </summary>
    Task<string?> DetectAnomalyAsync(JsonElement root);
}