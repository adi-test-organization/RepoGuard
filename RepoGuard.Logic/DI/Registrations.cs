using Microsoft.Extensions.DependencyInjection;
using RepoGuard.Core.Interfaces;
using RepoGuard.Logic.Services;

namespace RepoGuard.Logic.DI;

public static class Registrations
{
    public static IServiceCollection AddNotificationService(this IServiceCollection services)
    {
        return services.AddSingleton<INotificationService, ConsoleNotificationService>();
    }

    public static IServiceCollection AddAnomalyDetectors(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        var currentAssembly = typeof(Registrations).Assembly;
        var detectorInterface = typeof(IAnomalyDetector);
        var detectors = 
            currentAssembly
            .GetTypes()
            .Where(p => detectorInterface.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        foreach (var detector in detectors)
        {
            services.AddSingleton(detectorInterface, detector);
        }
        return services;
    }
}
