namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;

public class DataHealthActionRepository(ActionDBContext actionDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<DataHealthActionWrapper>(requestHeaderContext)
{
    protected override DbContext DBContext => actionDbContext;
    protected override DbSet<DataHealthActionWrapper> DBSet => actionDbContext.Actions;
}
