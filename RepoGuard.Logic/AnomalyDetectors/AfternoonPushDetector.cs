using RepoGuard.Core.Constants;
using RepoGuard.Core.Interfaces;
using System.Text.Json;

namespace RepoGuard.Detectors;

internal class AfternoonPushDetector : IAnomalyDetector
{
    public bool CanHandle(string eventType) => eventType.Equals(GithubConstants.Events.Push, StringComparison.OrdinalIgnoreCase);

    public Task<string?> DetectAnomalyAsync(JsonElement root)
    {
        if (!root.TryGetProperty(GithubConstants.JsonProperties.Repository, out var repo) ||
            !repo.TryGetProperty(GithubConstants.JsonProperties.PushedAt, out var pushedElement) ||
            !pushedElement.TryGetInt64(out long pushedUnixTime))
        {
            return Task.FromResult<string?>(null);
        }

        var pushTimeUtc = DateTimeOffset.FromUnixTimeSeconds(pushedUnixTime);

        TimeSpan userOffset = TimeSpan.Zero;

        if (root.TryGetProperty(GithubConstants.JsonProperties.HeadCommit, out var headCommit) &&
            headCommit.TryGetProperty(GithubConstants.JsonProperties.Timestamp, out var timestampElem) &&
            DateTimeOffset.TryParse(timestampElem.GetString(), out var commitTime))
        {
            userOffset = commitTime.Offset;
        }

        var userLocalPushTime = pushTimeUtc.ToOffset(userOffset);

        var hour = userLocalPushTime.Hour;
        if (hour >= 14 && hour < 16)
        {
            var pusher = root.GetProperty(GithubConstants.JsonProperties.Pusher).GetProperty(GithubConstants.JsonProperties.Name).GetString();
            return Task.FromResult<string?>($"Anomaly: '{pusher}' pushed code at {userLocalPushTime:HH:mm} (UTC{userOffset.TotalHours:+0;-0}) - local time.");
        }

        return Task.FromResult<string?>(null);
    }
}