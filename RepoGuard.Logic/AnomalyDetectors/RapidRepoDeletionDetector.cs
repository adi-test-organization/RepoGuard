using RepoGuard.Core.Constants;
using RepoGuard.Core.Interfaces;
using System.Text.Json;

namespace RepoGuard.Detectors;

public class RapidRepoDeletionDetector : IAnomalyDetector
{
    public bool CanHandle(string eventType) => eventType.Equals(GithubConstants.Events.Repository, StringComparison.OrdinalIgnoreCase);

    public Task<string?> DetectAnomalyAsync(JsonElement root)
    {
        if (!root.TryGetProperty(GithubConstants.JsonProperties.Action, out var actionProp) ||
            !actionProp.ValueEquals(GithubConstants.Actions.Deleted))
        {
            return Task.FromResult<string?>(null);
        }

        if (root.TryGetProperty(GithubConstants.JsonProperties.Repository, out var repoProp))
        {
            var repoName = repoProp.GetProperty(GithubConstants.JsonProperties.FullName).GetString() ?? "Unknown";

            if (repoProp.TryGetProperty(GithubConstants.JsonProperties.CreatedAt, out var createdAtProp) &&
                createdAtProp.TryGetDateTimeOffset(out var createdAt) &&
                repoProp.TryGetProperty(GithubConstants.JsonProperties.UpdatedAt, out var updatedAtProp) &&
                updatedAtProp.TryGetDateTimeOffset(out var deletionTime))
            {
                var lifespan = deletionTime - createdAt;

                if (lifespan.TotalMinutes < 10)
                {
                    return Task.FromResult<string?>($"Anomaly Detected: Repository '{repoName}' was deleted only {lifespan.TotalMinutes:F1} minutes after creation.");
                }
            }
        }

        return Task.FromResult<string?>(null);
    }

}