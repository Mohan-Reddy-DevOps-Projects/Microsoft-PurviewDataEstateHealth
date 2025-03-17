namespace DEH.Application;

using Microsoft.Extensions.DependencyInjection;

public static class InitializeServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(InitializeServices).Assembly);
        });

        return services;
    }
}