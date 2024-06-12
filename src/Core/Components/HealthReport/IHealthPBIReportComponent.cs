// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

internal interface IHealthPBIReportComponent
{
    Task CreateDataGovernanceReport(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, CancellationToken cancellationToken, bool update = false);
    Task CreateDataQualityReport(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, CancellationToken cancellationToken, bool update = false);
    Task<Dataset> CreateDataset(Guid profileId, Guid workspaceId, IDatasetRequest sharedDatasetRequest, CancellationToken cancellationToken, bool update = false);
    Task<Report> CreateReport(Dataset sharedDataset, IDatasetRequest reportRequest, CancellationToken cancellationToken, bool update = false);
    IDatasetRequest GetReportRequest(Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, string reportName);
    IDatasetRequest GetSQLSharedDatasetRequest(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, string sharedDatasetName);
}