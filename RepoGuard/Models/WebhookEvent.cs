namespace RepoGuard.Models;

public record WebhookEvent(string EventType, string Payload);