// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
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
        /// <returns></returns>
        void TriggerMDQJobCallback(MDQJobModel jobModel, bool isRetry);
    }
}