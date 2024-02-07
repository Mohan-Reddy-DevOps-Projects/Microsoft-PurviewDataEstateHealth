namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DataHealthActionRepository(ActionDBContext actionDbContext, IRequestHeaderContext requestHeaderContext) : CommonRepository<DataHealthActionWrapper>(requestHeaderContext)
{
    protected override DbContext DBContext => actionDbContext;

    public async Task<DataHealthActionWrapper?> GetActionByFilterAsync(DataHealthActionCategory category, string findingType, string findingSubType, string findingId, DataHealthActionTargetEntityType targetType, string targetId)
    {
        return await this.DBContext.Set<DataHealthActionWrapper>().WithPartitionKey(this.TenantId)
            .Where(
                x => x.Category == category &&
                x.FindingType == findingType &&
                x.FindingSubType == findingSubType &&
                x.FindingId == findingId &&
                x.TargetEntityType == targetType &&
                x.TargetEntityId == x.TargetEntityId)
            .SingleOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<GroupedActions>> EnumerateActionsByGroupAsync(string groupBy)
    {
        // TODO: need a more efficient way
        var actions = await this.GetAllAsync().ConfigureAwait(false);
        return GroupedActions.ToGroupedActions(groupBy, actions);
    }
}
