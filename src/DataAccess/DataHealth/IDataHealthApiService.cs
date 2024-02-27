// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using System.Threading.Tasks;

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
        /// <param name="payload"></param>
        /// <returns></returns>
        Task TriggerMDQJobCallback(MDQJobCallbackPayload payload);
    }
}