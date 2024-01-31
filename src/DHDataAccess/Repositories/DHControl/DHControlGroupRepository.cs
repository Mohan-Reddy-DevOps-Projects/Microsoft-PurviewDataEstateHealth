#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;

public class DHControlGroupRepository(ControlDBContext controlDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<DHControlGroup>(requestHeaderContext)
{
    protected override DbContext TheDbContext => controlDbContext;
    protected override DbSet<DHControlGroup> TheDbSet => controlDbContext.DHControlGroups;
}