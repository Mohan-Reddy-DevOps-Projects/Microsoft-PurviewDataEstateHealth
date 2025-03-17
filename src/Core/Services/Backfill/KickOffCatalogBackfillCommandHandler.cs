namespace DEH.Application.Backfill;

using Abstractions.Catalog;
using Domain.Abstractions;
using Messaging;
using Microsoft.Azure.Purview.DataEstateHealth.Core;

internal sealed class KickOffCatalogBackfillCommandHandler : ICommandHandler<KickOffCatalogBackfillCommand, string>
{
    private readonly IJobManager _backgroundJobManager;

    public KickOffCatalogBackfillCommandHandler(ICatalogHttpClientFactory catalogHttpClientFactory, IJobManager backgroundJobManager)
    {
        this._backgroundJobManager = backgroundJobManager;
    }

    public async Task<Result<string>> Handle(KickOffCatalogBackfillCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await this._backgroundJobManager.RunCatalogBackfillJob(
                request.AccountIds,
                request.BatchAmount,
                request.BufferTimeInMinutes);

            return Result.Success($"Catalog backfill job started successfully with {request.AccountIds.Count} accounts");
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(new Error("CatalogBackfill.Failed", ex.Message));
        }
    }
}