namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

public interface IAssessmentRepositoryFactory
{
    DHAssessmentRepository CreateAssessmentRepository();
} 