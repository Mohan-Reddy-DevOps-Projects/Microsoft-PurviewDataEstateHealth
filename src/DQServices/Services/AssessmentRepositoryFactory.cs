namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using System;

public class AssessmentRepositoryFactory(IServiceProvider serviceProvider) : IAssessmentRepositoryFactory
{
    public DHAssessmentRepository CreateAssessmentRepository()
    {
        using var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DHAssessmentRepository>();
    }
} 