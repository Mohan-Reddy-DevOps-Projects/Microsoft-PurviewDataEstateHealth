// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.PowerBI.Api.Models.Credentials;
using Microsoft.Rest;

/// <summary>
/// Power BI service
/// </summary>
internal class PowerBIService : IPowerBIService
{
    private readonly PowerBIFactory powerBIFactory;
    private readonly PowerBIAuthConfiguration authConfiguration;
    private readonly IDataEstateHealthLogger logger;

    private const string Tag = nameof(PowerBIService);

    public PowerBIService(
        PowerBIFactory powerBIFactory,
        IOptions<PowerBIAuthConfiguration> authConfiguration,
        IDataEstateHealthLogger logger)
    {
        this.powerBIFactory = powerBIFactory;
        this.authConfiguration = authConfiguration.Value;
        this.logger = logger;
    }

    /// <summary>
    /// Initialize the Power BI client
    /// </summary>
    /// <returns></returns>
    public async Task Initialize()
    {
        if (this.authConfiguration.Enabled)
        {
            await this.powerBIFactory.GetClientAsync(CancellationToken.None);
        }
    }

    #region Profile

    /// <inheritdoc/>
    public async Task<ServicePrincipalProfile> CreateProfile(string profileName, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        CreateOrUpdateProfileRequest request = new()
        {
            DisplayName = profileName,
        };
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Profile, PowerBIOperations.Create);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Profile} {PowerBIOperations.Create}: Body: {JsonSerializer.Serialize(request)}");
        using HttpOperationResponse<ServicePrincipalProfile> response = await pbiClient.Profiles.CreateProfileWithHttpMessagesAsync(request, cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Body;
    }

    /// <inheritdoc/>
    public async Task DeleteProfile(Guid profileId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Profile, PowerBIOperations.Delete);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Profile} {PowerBIOperations.Delete}: ProfileId: {profileId}");
        using HttpOperationResponse response = await pbiClient.Profiles.DeleteProfileWithHttpMessagesAsync(profileId, cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);
    }

    /// <inheritdoc/>
    public async Task<ServicePrincipalProfiles> GetProfiles(CancellationToken cancellationToken, int? top = null, int? skip = null, string filter = null)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Profile, PowerBIOperations.List);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Profile} {PowerBIOperations.List}: top: {top}; skip: {skip}; filter: {filter}");
        using HttpOperationResponse<ServicePrincipalProfiles> response = await pbiClient.Profiles.GetProfilesWithHttpMessagesAsync(top, skip, filter, cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Body;
    }

    #endregion

    #region Workspace

    /// <inheritdoc/>
    public async Task<Group> CreateWorkspace(Guid profileId, string workspaceName, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        GroupCreationRequest request = new()
        {
            Name = workspaceName,
        };
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Workspace, PowerBIOperations.Get);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Workspace} {PowerBIOperations.Get}: ProfileId: {profileId}; Body: {JsonSerializer.Serialize(request)}");
        using HttpOperationResponse<Group> response = await pbiClient.Groups.CreateGroupWithHttpMessagesAsync(request, true, GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Body;
    }

    /// <inheritdoc/>
    public async Task<Groups> GetWorkspaces(Guid profileId, CancellationToken cancellationToken, string filter = null, int? top = null, int? skip = null)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Workspace, PowerBIOperations.List);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Workspace} {PowerBIOperations.List}: ProfileId: {profileId}");
        using HttpOperationResponse<Groups> response = await pbiClient.Groups.GetGroupsWithHttpMessagesAsync(filter: filter, top: top, skip: skip, customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Body;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> DeleteWorkspace(Guid profileId, Guid workspaceId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Workspace, PowerBIOperations.Delete);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Workspace} {PowerBIOperations.Delete}: ProfileId: {profileId}; WorkspaceId: {workspaceId}");
        using HttpOperationResponse response = await pbiClient.Groups.DeleteGroupWithHttpMessagesAsync(workspaceId, customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Response;
    }

    #endregion

    #region Token

    /// <inheritdoc/>
    public async Task<EmbedToken> GenerateEmbeddedToken(EmbeddedTokenRequest embeddedTokenRequest, CancellationToken cancellationToken)
    {
        if (embeddedTokenRequest.DatasetIds.Length is < 1 or > 50)
        {
            throw new ServiceError(
                ErrorCategory.ServiceError,
                ServiceErrorCode.InvalidField.Code,
                "Between 1 and 50 datasets are are allowed when generating an embedded token.")
                .ToException();
        }
        if (embeddedTokenRequest.ReportIds.Length is < 1 or > 50)
        {
            throw new ServiceError(
                ErrorCategory.ServiceError,
                ServiceErrorCode.InvalidField.Code,
                "Between 1 and 50 reports are are allowed when generating an embedded token.")
                .ToException();
        }
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        GenerateTokenRequestV2 request = embeddedTokenRequest.ToResource();
        Dictionary<string, string> dimensions = GetDimensions(EntityType.EmbedToken, PowerBIOperations.Create);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.EmbedToken} {PowerBIOperations.Create}: Body: {JsonSerializer.Serialize(request)}");
        using HttpOperationResponse<EmbedToken> response = await pbiClient.EmbedToken.GenerateTokenWithHttpMessagesAsync(request, GetProfileHeader(embeddedTokenRequest.ProfileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Body;
    }

    #endregion

    #region Report

    /// <inheritdoc/>
    public async Task<Reports> GetReports(Guid profileId, Guid groupId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);

        Dictionary<string, string> dimensions = GetDimensions(EntityType.Report, PowerBIOperations.List);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Report} {PowerBIOperations.List}: ProfileId: {profileId}; WorkspaceId: {groupId}");
        using HttpOperationResponse<Reports> response = await pbiClient.Reports.GetReportsInGroupWithHttpMessagesAsync(groupId, GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Body;
    }

    /// <inheritdoc/>
    public async Task<Report> GetReport(Guid profileId, Guid groupId, Guid reportId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);

        Dictionary<string, string> dimensions = GetDimensions(EntityType.Report, PowerBIOperations.Get);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Report} {PowerBIOperations.Get}: ProfileId: {profileId}; WorkspaceId: {groupId}");
        using HttpOperationResponse<Report> response = await pbiClient.Reports.GetReportInGroupWithHttpMessagesAsync(groupId, reportId, GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Body;
    }

    #endregion

    #region Capacity

    /// <inheritdoc/>
    public async Task AssignWorkspaceCapacity(Guid profileId, Guid groupId, Guid capacityId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        AssignToCapacityRequest request = new()
        {
            CapacityId = capacityId
        };
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Workspace, PowerBIOperations.AssignCapacity);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Workspace} {PowerBIOperations.AssignCapacity}: WorkspaceId: {groupId}; ProfileId: {profileId}; Body: {JsonSerializer.Serialize(request)}");
        using HttpOperationResponse response = await pbiClient.Groups.AssignToCapacityWithHttpMessagesAsync(groupId, request, GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);
    }

    /// <inheritdoc/>
    public async Task<Capacities> ListCapacities(Guid profileId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Capacity, PowerBIOperations.List);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Capacity} {PowerBIOperations.List}: ProfileId: {profileId}");
        using HttpOperationResponse<Capacities> response = await pbiClient.Capacities.GetCapacitiesWithHttpMessagesAsync(GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Body;
    }

    #endregion

    #region Dataset

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> DeleteDataset(Guid profileId, Guid workspaceId, Guid datasetId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Dataset, PowerBIOperations.Delete);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Dataset} {PowerBIOperations.Delete}: ProfileId: {profileId}; WorkspaceId: {workspaceId}; DatasetId: {datasetId}");
        using HttpOperationResponse response = await pbiClient.Datasets.DeleteDatasetInGroupWithHttpMessagesAsync(workspaceId, datasetId.ToString(), customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);

        return response.Response;
    }

    /// <inheritdoc/>
    public async Task<Datasets> GetDatasets(Guid profileId, Guid workspaceId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Dataset, PowerBIOperations.List);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Dataset} {PowerBIOperations.List}: ProfileId: {profileId}; WorkspaceId: {workspaceId}");
        using HttpOperationResponse<Datasets> response = await pbiClient.Datasets.GetDatasetsInGroupWithHttpMessagesAsync(workspaceId, customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);
        this.logger.LogTrace($"{Tag}|{EntityType.Dataset} {PowerBIOperations.List}\n{JsonSerializer.Serialize(response.Body)}");

        return response.Body;
    }

    /// <inheritdoc/>
    public async Task<Dataset> GetDataset(Guid profileId, Guid workspaceId, Guid datasetId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Dataset, PowerBIOperations.Get);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Dataset} {PowerBIOperations.Get}: ProfileId: {profileId}; WorkspaceId: {workspaceId}");
        using HttpOperationResponse<Dataset> response = await pbiClient.Datasets.GetDatasetInGroupWithHttpMessagesAsync(workspaceId, datasetId.ToString(), customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);
        this.logger.LogTrace($"{Tag}|{EntityType.Dataset} {PowerBIOperations.List}\n{JsonSerializer.Serialize(response.Body)}");

        return response.Body;
    }

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
    /// <returns></returns>
    public async Task<Import> CreateDataset(Guid profileId, Guid workspaceId, Stream stream, string datasetName, Dictionary<string, string> parameters, PowerBICredential powerBiCredential, CancellationToken cancellationToken, bool optimizedDataset = false)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);

        Dictionary<string, string> dimensions = GetDimensions(EntityType.Import, PowerBIOperations.Create);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Import} {PowerBIOperations.Create}: ProfileId: {profileId}; WorkspaceId: {workspaceId}; DatasetName: {datasetName}");
        using HttpOperationResponse<Import> response = await pbiClient.Imports.PostImportFileWithHttpMessage(workspaceId, stream, datasetName, customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);
        this.logger.LogTrace($"{Tag}|{EntityType.Import} {PowerBIOperations.Create}\n{JsonSerializer.Serialize(response.Body)}");

        Import postResponse = response.Body;

        static bool exceptionPredicate(Exception ex) => ex is ServiceException rex && rex.ServiceError.Category == ErrorCategory.ServiceError && rex.ServiceError.Code == ErrorCode.PowerBI_ImportNotCompleted.Code;
        Import import = await RetryUtil.ExecuteWithRetryAsync<Import, Exception>(async retryCount =>
        {
            Dictionary<string, string> dimensions = GetDimensions(EntityType.Import, PowerBIOperations.Get);
            this.logger.LogTrace($"{Tag}|Request - {EntityType.Import} {PowerBIOperations.Get}: ProfileId: {profileId}; WorkspaceId: {workspaceId}; ImportId: {postResponse.Id}");
            using HttpOperationResponse<Import> response = await pbiClient.Imports.GetImportInGroupWithHttpMessagesAsync(workspaceId, postResponse.Id, customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
            this.LogResponse(response, dimensions);

            Import import = response.Body;

            if (import.ImportState is ImportState.Succeeded or ImportState.Failed)
            {
                return import;
            }

            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.PowerBI_ImportNotCompleted.Code, $"Import state is {import.ImportState}").ToException();

        }, exceptionPredicate: exceptionPredicate, maxRetries: 10, retryIntervalInMs: 5000);

        if (import.ImportState == ImportState.Succeeded)
        {
            foreach (Dataset dataset in import.Datasets)
            {
                if (optimizedDataset)
                {
                    // update to premium dataset
                    await this.UpdateDatasetStorageMode(profileId, dataset.Id, workspaceId, StorageMode.PremiumFiles, cancellationToken);
                }
                await this.UpdateDatasetConnectionString(profileId, Guid.Parse(dataset.Id), workspaceId, parameters, powerBiCredential, cancellationToken);
            }

            return import;
        }

        throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.PowerBI_ImportFailed.Code, $"Import state is {import.ImportState}").ToException();
    }

    #endregion

    private static Dictionary<string, List<string>> GetProfileHeader(Guid profileId)
    {
        return new Dictionary<string, List<string>>()
            {
                {
                    "X-PowerBI-Profile-Id", new List<string>()
                    {
                        profileId.ToString()
                    }
                }
            };
    }

    /// <summary>
    /// Update the dataset with the provided storage mode.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="datasetId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="datasetStorageMode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateDatasetStorageMode(Guid profileId, string datasetId, Guid workspaceId, StorageMode datasetStorageMode, CancellationToken cancellationToken)
    {
        UpdateDatasetRequest updateRequest = new()
        {
            TargetStorageMode = datasetStorageMode.ToString()
        };
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        Dictionary<string, string> dimensions = GetDimensions(EntityType.Dataset, PowerBIOperations.Update);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Dataset} {PowerBIOperations.Update}: WorkspaceId: {workspaceId}; DatasetId: {datasetId}; ProfileId: {profileId}; Body: {JsonSerializer.Serialize(updateRequest)}");
        using HttpOperationResponse response = await pbiClient.Datasets.UpdateDatasetInGroupWithHttpMessagesAsync(workspaceId, datasetId, updateRequest, customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);
    }

    /// <summary>
    /// Get the datasources of a given dataset
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="datasetId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Datasources> GetDataSources(Guid profileId, Guid datasetId, Guid workspaceId, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);

        Dictionary<string, string> dimensions = GetDimensions(EntityType.Datasource, PowerBIOperations.List);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Datasource} {PowerBIOperations.List}: ProfileId: {profileId}; WorkspaceId: {workspaceId}; DatasetId: {datasetId}");
        using HttpOperationResponse<Datasources> response = await pbiClient.Datasets.GetDatasourcesInGroupWithHttpMessagesAsync(workspaceId, datasetId.ToString(), customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
        this.LogResponse(response, dimensions);
        this.logger.LogTrace($"{Tag}|{EntityType.Datasource} {PowerBIOperations.List}\n{JsonSerializer.Serialize(response.Body)}");
        return response.Body;
    }

    /// <summary>
    /// Updates data source connection string
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="datasetId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="parameters"></param>
    /// <param name="powerBiCredential"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task UpdateDatasetConnectionString(Guid profileId, Guid datasetId, Guid workspaceId, Dictionary<string, string> parameters, PowerBICredential powerBiCredential, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);

        try
        {
            UpdateMashupParametersRequest request = new()
            {
                UpdateDetails = new List<UpdateMashupParameterDetails>()
            };
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                request.UpdateDetails.Add(new UpdateMashupParameterDetails()
                {
                    Name = parameter.Key,
                    NewValue = parameter.Value
                });
            }
            Dictionary<string, string> dimensions = GetDimensions(EntityType.Dataset, PowerBIOperations.Update);
            this.logger.LogTrace($"{Tag}|Request - {EntityType.Dataset} {PowerBIOperations.Update}: WorkspaceId: {workspaceId}; DatasetId: {datasetId}; ProfileId: {profileId}; Body: {JsonSerializer.Serialize(request)}");
            using HttpOperationResponse response = await pbiClient.Datasets.UpdateParametersInGroupWithHttpMessagesAsync(workspaceId, datasetId.ToString(), request, customHeaders: GetProfileHeader(profileId), cancellationToken: cancellationToken);
            this.LogResponse(response, dimensions);

            Datasources dataSourcesList = await this.GetDataSources(profileId, datasetId, workspaceId, cancellationToken);
            if (dataSourcesList.Value.Count <= 0)
            {
                throw new HttpOperationException();
            }

            foreach (Datasource dataSource in dataSourcesList.Value)
            {
                if (dataSource.DatasourceId == null || dataSource.GatewayId == null)
                {
                    throw new HttpOperationException();
                }
                await this.SetCredentials(profileId, (Guid)dataSource.GatewayId, (Guid)dataSource.DatasourceId, powerBiCredential, cancellationToken);
            }
        }
        catch (HttpOperationException ex)
        {
            this.logger.LogError($"PowerBIEmbeddedServiceManager: UpdateDatasetConnectionString - StatusCode: {ex.Response.StatusCode}; Content: {ex.Response.Content}", ex);

            // Delete the dataset if any of the steps above fails.
            // By deleting the dataset, both the dataset and related report will be deleted.
            await this.DeleteDataset(profileId, workspaceId, datasetId, cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Updates the credentials of the specified data source from the specified gateway.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="gatewayId"></param>
    /// <param name="dataSourceId"></param>
    /// <param name="powerBiCredential"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task SetCredentials(Guid profileId, Guid gatewayId, Guid dataSourceId, PowerBICredential powerBiCredential, CancellationToken cancellationToken)
    {
        PowerBIClient pbiClient = await this.powerBIFactory.GetClientAsync(cancellationToken);
        string loginName = powerBiCredential.LoginName;
        string pwd = powerBiCredential.Password.ToPlainString();
        BasicCredentials credentials = new(loginName, pwd);
        UpdateDatasourceRequest updateDatasourceRequest = new(new CredentialDetails(credentials, PrivacyLevel.None, EncryptedConnection.Encrypted));

        Dictionary<string, string> dimensions = GetDimensions(EntityType.Gateway, PowerBIOperations.Update);
        this.logger.LogTrace($"{Tag}|Request - {EntityType.Gateway} {PowerBIOperations.Update}: GatewayId: {gatewayId}; DataSourceId: {dataSourceId}; ProfileId: {profileId}");
        using HttpOperationResponse response = await pbiClient.Gateways.UpdateDatasourceWithHttpMessagesAsync(gatewayId, dataSourceId, updateDatasourceRequest, customHeaders: GetProfileHeader(profileId), cancellationToken);
        this.LogResponse(response, dimensions);
    }

    #region PBI Operation Metrics

    private void LogResponse<T>(HttpOperationResponse<T> response, Dictionary<string, string> dimensions)
    {
        this.LogResponse(response, dimensions, null);
    }

    private void LogResponse(HttpOperationResponse response, Dictionary<string, string> dimensions, object unused = null)
    {
        string headers = FormatHttpResponseHeaders(response.Response.Headers);
        string stringifiedDimensions = FormattedDimensions(dimensions);
        dimensions[PowerBIOperationDimensions.StatusCode] = response.Response.StatusCode.ToString();
        this.logger.LogTrace($"{Tag}|{stringifiedDimensions}; ResponseHeaders:{headers}; StatusCode:{response.Response.StatusCode}");
    }

    /// <summary>
    /// Formats the response headers into a string.
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    private static string FormatHttpResponseHeaders(IDictionary<string, IEnumerable<string>> headers) => string.Join(", ", headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"));

    /// <summary>
    /// Formats the response headers into a string.
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    private static string FormatHttpResponseHeaders(HttpResponseHeaders headers) => string.Join(", ", headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"));

    /// <summary>
    /// Formats a dictionary of dimensions into a string.
    /// </summary>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    private static string FormattedDimensions(Dictionary<string, string> dimensions) => string.Join(", ", dimensions.Select(kv => $"{kv.Key}: {kv.Value}"));

    private static Dictionary<string, string> GetDimensions(
        string entityType,
        string operation)
    {
        return new Dictionary<string, string>
        {
            { PowerBIOperationDimensions.EntityType, entityType },
            { PowerBIOperationDimensions.Operation, operation },
        };
    }

    private class PowerBIOperationDimensions
    {
        public const string EntityType = "EntityType";
        public const string Operation = "Operation";
        public const string StatusCode = "StatusCode";
    }

    private class PowerBIOperations
    {
        public const string AssignCapacity = "AssignCapacity";
        public const string Create = "Create";
        public const string Delete = "Delete";
        public const string Get = "Get";
        public const string List = "List";
        public const string Update = "Update";
        public const string Status = "Status";
        public const string OnDemand = "OnDemand";
    }

    private class EntityType
    {
        public const string Capacity = "Capacity";
        public const string Dataset = "Dataset";
        public const string Datasource = "Datasource";
        public const string EmbedToken = "EmbedToken";
        public const string Gateway = "Gateway";
        public const string Import = "Import";
        public const string Profile = "Profile";
        public const string Report = "Report";
        public const string Refresh = "Refresh";
        public const string RefreshHistory = "RefreshHistory";
        public const string Workspace = "Workspace";
    }

    #endregion
}
