// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.PowerBI.Api.Models;

internal sealed class ReportCommand : IReportCommand
{
    private readonly IDataEstateHealthLogger logger;
    private readonly IPowerBIService powerBIService;
    private readonly IDatasetCommand datasetCommand;

    public ReportCommand(IDataEstateHealthLogger logger, IPowerBIService powerBIService, IDatasetCommand datasetCommand)
    {
        this.logger = logger;
        this.powerBIService = powerBIService;
        this.datasetCommand = datasetCommand;
    }

    /// <inheritdoc/>
    public async Task<Report> Get(IReportRequest requestContext, CancellationToken cancellationToken)
    {
        if (requestContext.ProfileId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ProfileId.").ToException();
        }
        if (requestContext.WorkspaceId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing WorkspaceId.").ToException();
        }
        if (requestContext.ReportId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ReportId.").ToException();
        }

        return await this.powerBIService.GetReport(requestContext.ProfileId, requestContext.WorkspaceId, requestContext.ReportId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Reports> List(IReportRequest requestContext, CancellationToken cancellationToken)
    {
        if (requestContext.ProfileId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing ProfileId.").ToException();
        }
        if (requestContext.WorkspaceId == Guid.Empty)
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.MissingField.Code, "Missing WorkspaceId.").ToException();
        }

        return await this.powerBIService.GetReports(requestContext.ProfileId, requestContext.WorkspaceId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Report> Bind(Dataset targetDataset, IDatasetRequest requestContext, CancellationToken cancellationToken)
    {
        Import import = await this.datasetCommand.Import(requestContext, cancellationToken);
        Dataset dataset = import.Datasets.First();
        Report report = import.Reports.First();

        await this.powerBIService.RebindReport(requestContext.ProfileId, requestContext.WorkspaceId, Guid.Parse(targetDataset.Id), report.Id, cancellationToken);
        IDatasetRequest deleteRequest = new DatasetRequest()
        {
            DatasetId = Guid.Parse(dataset.Id),
            ProfileId = requestContext.ProfileId,
            WorkspaceId = requestContext.WorkspaceId
        };
        await this.datasetCommand.Delete(deleteRequest, cancellationToken);

        return report;
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> Delete(IReportRequest requestContext, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await this.powerBIService.DeleteReport(requestContext.ProfileId, requestContext.WorkspaceId, requestContext.ReportId, cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.NoContent or HttpStatusCode.OK => new DeletionResult()
            {
                DeletionStatus = DeletionStatus.Deleted
            },
            HttpStatusCode.NotFound => new DeletionResult()
            {
                DeletionStatus = DeletionStatus.ResourceNotFound
            },
            _ => new DeletionResult()
            {
                DeletionStatus = DeletionStatus.Unknown
            },
        };
    }
}
