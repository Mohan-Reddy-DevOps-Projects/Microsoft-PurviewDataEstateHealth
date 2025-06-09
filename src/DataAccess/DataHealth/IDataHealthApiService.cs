// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
    using Microsoft.Azure.ProjectBabylon.Metadata.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.JobManagerModels;

    /// <summary>
    /// Result of creating data quality rules specification
    /// </summary>
    /// <param name="Success">Whether the data quality rules specification was created successfully</param>
    /// <param name="ControlsWorkflowId">The specification ID returned from the creation operation</param>
    public record CreateDataQualityRulesSpecResult(bool Success, string ControlsWorkflowId);

    /// <summary>
    /// Metadata service accessor
    /// </summary>
    public interface IDataHealthApiService
    {
        /// <summary>
        /// Initialize the service.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets a sas token for the processing storage account.
        /// </summary>
        /// <param name="jobModel"></param>
        /// <param name="isRetry"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        void TriggerMDQJobCallback(MDQJobModel jobModel, bool isRetry, CancellationToken cancellationToken);

        /// <summary>
        /// Trigger DEH schedule
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TriggerDEHSchedule(TriggeredSchedulePayload payload, CancellationToken cancellationToken);

        /// <summary>
        /// Create data quality rules specification
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result containing success status and specification ID</returns>
        Task<CreateDataQualityRulesSpecResult> CreateDataQualityRulesSpec(TriggeredSchedulePayload payload, CancellationToken cancellationToken);

        /// <summary>
        /// Trigger actions creation workflow
        /// </summary>
        /// <param name="jobModel"></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task for the API call</returns>
        Task TriggerActionsUpsert(MDQJobModel jobModel, string traceId, CancellationToken cancellationToken);

        /// <summary>
        /// Clean up actions.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CleanUpActionsJobCallback(AccountServiceModel account, CancellationToken cancellationToken);


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<string> GetMIToken(string accountId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<StorageConfiguration> GetStorageConfigSettings(string accountId, string tenantId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Task<string> GetDEHSKUConfig(string accountId);

    }
}