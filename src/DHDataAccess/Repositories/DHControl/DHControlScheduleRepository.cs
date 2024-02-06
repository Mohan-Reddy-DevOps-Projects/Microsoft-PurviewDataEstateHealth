namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

public class DHControlScheduleRepository(ControlDBContext controlDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<DHControlScheduleWrapper>(requestHeaderContext)
{
    protected override DbContext DBContext => controlDbContext;
}