namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

public class DHScoreRepository(ControlDBContext controlDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<DHScoreWrapper>(requestHeaderContext)
{
    protected override DbContext DBContext => controlDbContext;
    protected override DbSet<DHScoreWrapper> DBSet => controlDbContext.DHScores;
}