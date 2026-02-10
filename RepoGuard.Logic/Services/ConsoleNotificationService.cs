using RepoGuard.Core.Interfaces;

namespace RepoGuard.Logic.Services;

internal class ConsoleNotificationService : INotificationService
{
    public Task NotifyAsync(string message)
    {
        lock (Console.Out)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.ForegroundColor = originalColor;
        }
        return Task.CompletedTask;
    }
}