namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;

public class MQAssessmentRepository(ControlDBContext controlDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<MQAssessmentWrapper>(requestHeaderContext)
{
    protected override DbContext DBContext => controlDbContext;
    protected override DbSet<MQAssessmentWrapper> DBSet => controlDbContext.MQAssessments;
}