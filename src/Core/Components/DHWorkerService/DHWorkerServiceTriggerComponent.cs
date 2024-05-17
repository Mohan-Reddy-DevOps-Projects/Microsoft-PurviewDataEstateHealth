// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using System.Threading;
using System.Threading.Tasks;

[Component(typeof(IDHWorkerServiceTriggerComponent), ServiceVersion.V1)]
internal sealed class DHWorkerServiceTriggerComponent : BaseComponent<IDHWorkerServiceTriggerContext>, IDHWorkerServiceTriggerComponent
{
#pragma warning disable 649
    [Inject]
    private readonly IJobManager backgroundJobManager;

#pragma warning restore 649

    private IDHWorkerServiceTriggerContext context;

    public DHWorkerServiceTriggerComponent(IDHWorkerServiceTriggerContext context, int version) : base(context, version)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task TriggerBackgroundJob(string jobPartition, string jobId, CancellationToken cancellationToken)
    {
        await this.backgroundJobManager.TriggerBackgroundJobAsync(jobPartition, jobId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, string>> GetBackgroundJob(string jobPartition, string jobId)
    {
        return await this.backgroundJobManager.GetBackgroundJobDetailAsync(jobPartition, jobId);
    }

    /// <inheritdoc/>
    public async Task UpsertDEHScheduleJob(DHControlScheduleWrapper schedule)
    {
        await this.backgroundJobManager.ProvisionDEHScheduleJob(this.context.TenantId.ToString(), this.context.AccountId.ToString(), schedule);
    }

    /// <inheritdoc/>
    public async Task DeleteDEHScheduleJob()
    {
        await this.backgroundJobManager.DeprovisionDEHScheduleJob(this.context.TenantId.ToString(), this.context.AccountId.ToString());
    }
}
