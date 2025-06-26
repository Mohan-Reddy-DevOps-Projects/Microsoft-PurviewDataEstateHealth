namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using System;

public class ControlRepositoryFactory(IServiceProvider serviceProvider) : IControlRepositoryFactory
{
    public DHControlRepository CreateControlRepository()
    {
        using var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DHControlRepository>();
    }
} 