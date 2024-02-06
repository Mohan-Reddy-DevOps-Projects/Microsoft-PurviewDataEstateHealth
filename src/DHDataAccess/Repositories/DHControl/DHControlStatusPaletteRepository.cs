namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;

public class DHControlStatusPaletteRepository(ControlDBContext controlDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<DHControlStatusPaletteWrapper>(requestHeaderContext)
{
    protected override DbContext DBContext => controlDbContext;
}
