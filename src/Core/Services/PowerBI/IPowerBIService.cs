// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Interface for PowerBI service
/// </summary>
public interface IPowerBIService
{
    /// <summary>
    /// Initialize the PowerBI client
    /// </summary>
    /// <returns></returns>
    Task Initialize();

    #region Profile

    /// <summary>
    /// Create a unique service principal profile in Power BI
    /// Each profile represents one customer in Power BI.
    /// </summary>
    /// <param name="profileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServicePrincipalProfile> CreateProfile(string profileName, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a service principal profile in Power BI
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteProfile(Guid profileId, CancellationToken cancellationToken);

    /// <summary>
    /// List the service principal profiles in PowerBI
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="top"></param>
    /// <param name="skip"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<ServicePrincipalProfiles> GetProfiles(CancellationToken cancellationToken, int? top = null, int? skip = null, string filter = null);

    #endregion

    #region Workspace

    /// <summary>
    /// Create a workspace in Power BI
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Group> CreateWorkspace(Guid profileId, string workspaceName, CancellationToken cancellationToken);

    /// <summary>
    /// List all workspaces in Power BI available to the specified profile
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="filter"></param>
    /// <param name="top"></param>
    /// <param name="skip"></param>
    /// <returns></returns>
    Task<Groups> GetWorkspaces(Guid profileId, CancellationToken cancellationToken, string filter = null, int? top = null, int? skip = null);

    /// <summary>
    /// Delete a workspace in Power BI that is available to the specified profile
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> DeleteWorkspace(Guid profileId, Guid workspaceId, CancellationToken cancellationToken);

    #endregion

    #region Token

    /// <summary>
    /// Generate an embedded token that will be used to render a report in the browser
    /// Limitations: https://learn.microsoft.com/en-us/rest/api/power-bi/embed-token/generate-token#limitations
    /// </summary>
    /// <param name="embeddedTokenRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<EmbedToken> GenerateEmbeddedToken(EmbeddedTokenRequest embeddedTokenRequest, CancellationToken cancellationToken);

    #endregion

    #region Capacity

    /// <summary>
    /// List the capacities available to the specified profile.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Capacities> ListCapacities(Guid profileId, CancellationToken cancellationToken);

    /// <summary>
    /// Assigns the specified workspace to an available capacity.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="groupId"></param>
    /// <param name="capacityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AssignWorkspaceCapacity(Guid profileId, Guid groupId, Guid capacityId, CancellationToken cancellationToken);

    #endregion

    #region Report

    /// <summary>
    /// Get the reports for the specified workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="groupId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Reports> GetReports(Guid profileId, Guid groupId, CancellationToken cancellationToken);

    /// <summary>
    /// Get the specified report from a workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="groupId"></param>
    /// <param name="reportId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Report> GetReport(Guid profileId, Guid groupId, Guid reportId, CancellationToken cancellationToken);

    /// <summary>
    /// Rebind the report to a dataset in the specified workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="groupId"></param>
    /// <param name="datasetId"></param>
    /// <param name="reportId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> RebindReport(Guid profileId, Guid groupId, Guid datasetId, Guid reportId, CancellationToken cancellationToken);

    /// <summary>
    /// Delete the report in the specified workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="groupId"></param>
    /// <param name="reportId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> DeleteReport(Guid profileId, Guid groupId, Guid reportId, CancellationToken cancellationToken);

    #endregion

    #region Dataset

    /// <summary>
    /// Delete a dataset in the specified workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="datasetId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> DeleteDataset(Guid profileId, Guid workspaceId, Guid datasetId, CancellationToken cancellationToken);

    /// <summary>
    /// Get the dataset in the specified workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="datasetId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Dataset> GetDataset(Guid profileId, Guid workspaceId, Guid datasetId, CancellationToken cancellationToken);

    /// <summary>
    /// Get the datasets for the specified workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Datasets> GetDatasets(Guid profileId, Guid workspaceId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a dataset in the specified workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="stream"></param>
    /// <param name="datasetName"></param>
    /// <param name="parameters"></param>
    /// <param name="powerBiCredential"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="optimizedDataset"></param>
    /// <param name="skipReport"></param>
    /// <returns></returns>
    Task<Import> CreateDataset(Guid profileId, Guid workspaceId, Stream stream, string datasetName, Dictionary<string, string> parameters, PowerBICredential powerBiCredential, CancellationToken cancellationToken, bool optimizedDataset = false, bool skipReport = false);

    #endregion

    #region Refresh

    /// <summary>
    /// Triggers a refresh for the specified dataset.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="datasetId"></param>
    /// <param name="refreshRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> OnDemandRefresh(Guid profileId, Guid workspaceId, Guid datasetId, DatasetRefreshRequest refreshRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Get the refresh history for the specified dataset.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="datasetId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="top"></param>
    /// <returns></returns>
    Task<Refreshes> GetRefreshHistory(Guid profileId, Guid workspaceId, Guid datasetId, CancellationToken cancellationToken, int? top = null);

    /// <summary>
    /// Returns execution details of an enhanced refresh operation for the specified dataset from the specified workspace.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="datasetId"></param>
    /// <param name="refreshId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DatasetRefreshDetail> GetRefreshStatus(Guid profileId, Guid workspaceId, Guid datasetId, Guid refreshId, CancellationToken cancellationToken);

    #endregion

}
