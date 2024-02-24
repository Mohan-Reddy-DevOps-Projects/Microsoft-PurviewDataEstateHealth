namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataGovernance.Reporting.Models;

internal class InMemoryProfileCache : LocalMemoryRepositoryCache<IHealthProfileCommand, IProfileModel, ProfileKey, ProfileKey>, IHealthProfileCommand
{
    public InMemoryProfileCache(IHealthProfileCommand repro, TimeSpan cacheTTLDuration, ICacheManager cacheManager, IDataEstateHealthRequestLogger logger)
        : base(repro, cacheTTLDuration, cacheManager, logger)
    {
    }
}
