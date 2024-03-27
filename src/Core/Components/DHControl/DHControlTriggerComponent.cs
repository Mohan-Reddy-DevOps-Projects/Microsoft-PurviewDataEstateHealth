// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using System.Threading;
using System.Threading.Tasks;

[Component(typeof(IDHControlTriggerComponent), ServiceVersion.V1)]
internal sealed class DHControlTriggerComponent : BaseComponent<IDHControlTriggerContext>, IDHControlTriggerComponent
{
#pragma warning disable 649
    [Inject]
    private readonly IJobManager backgroundJobManager;

    [Inject]
    private readonly IAccountExposureControlConfigProvider exposureControl;
#pragma warning restore 649

    public DHControlTriggerComponent(IDHControlTriggerContext context, int version) : base(context, version)
    {
    }

    /// <inheritdoc/>
    public async Task RefreshPowerBI(AccountServiceModel account, CancellationToken cancellationToken)
    {
        if (this.exposureControl.IsDGDataHealthEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            await this.backgroundJobManager.RunPBIRefreshJob(account);
        }
    }
}
