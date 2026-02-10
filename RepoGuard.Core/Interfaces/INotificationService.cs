namespace RepoGuard.Core.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(string message);
}