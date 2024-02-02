namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

public class DHControlRepository(ControlDBContext controlDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<DHControlBaseWrapper>(requestHeaderContext)
{
    protected override DbContext TheDbContext => controlDbContext;
    protected override DbSet<DHControlBaseWrapper> TheDbSet => controlDbContext.DHControls;
}