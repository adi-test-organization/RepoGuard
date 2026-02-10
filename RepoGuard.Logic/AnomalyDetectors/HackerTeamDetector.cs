using RepoGuard.Core.Constants;
using RepoGuard.Core.Interfaces;
using System.Text.Json;

namespace RepoGuard.Detectors;

public class HackerTeamDetector : IAnomalyDetector
{
    public bool CanHandle(string eventType) => eventType.Equals(GithubConstants.Events.Team, StringComparison.OrdinalIgnoreCase);

    public Task<string?> DetectAnomalyAsync(JsonElement root)
    {
        if (root.TryGetProperty(GithubConstants.JsonProperties.Action, out var action) && action.GetString() == GithubConstants.Actions.Created)
        {
            var teamName = root.GetProperty(GithubConstants.JsonProperties.Team).GetProperty(GithubConstants.JsonProperties.Name).GetString() ?? string.Empty;

            if (teamName.StartsWith("hacker", StringComparison.OrdinalIgnoreCase))
            {
                var sender = root.GetProperty(GithubConstants.JsonProperties.Sender).GetProperty(GithubConstants.JsonProperties.Login).GetString();
                return Task.FromResult<string?>($"Security Alert: User '{sender}' created a team named '{teamName}'.");
            }
        }
        return Task.FromResult<string?>(null);
    }
}