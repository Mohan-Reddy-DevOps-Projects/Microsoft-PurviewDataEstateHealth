// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
    using Microsoft.Azure.ProjectBabylon.Metadata.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;

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
        Task<bool> TriggerDEHScheduleCallback(TriggeredSchedulePayload payload, CancellationToken cancellationToken);

        /// <summary>
        /// Trigger DEH schedule
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TriggerDEHSchedule(TriggeredSchedulePayload payload, CancellationToken cancellationToken);

        /// <summary>
        /// Migrate schedule
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task MigrateSchedule(MigrateSchedulePayload payload, CancellationToken cancellationToken);

        /// <summary>
        /// Clean up actions.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> CleanUpActionsJobCallback(AccountServiceModel account, CancellationToken cancellationToken);
    }
}