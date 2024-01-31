#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;

public class DHControlNodeRepository(ControlDBContext controlDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<DHControlNode>(requestHeaderContext)
{
    protected override DbContext TheDbContext => controlDbContext;
    protected override DbSet<DHControlNode> TheDbSet => controlDbContext.DHControlNodes;
}