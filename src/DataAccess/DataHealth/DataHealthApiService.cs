// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using System;
    using System.Threading.Tasks;

    internal class DataHealthApiService : IDataHealthApiService
    {
        private readonly IDataEstateHealthRequestLogger logger;
        private readonly DataHealthApiServiceClientFactory apiServiceClientFactory;

        public DataHealthApiService(
            DataHealthApiServiceClientFactory apiServiceClientFactory,
            IDataEstateHealthRequestLogger logger)
        {
            this.apiServiceClientFactory = apiServiceClientFactory;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            this.GetDEHServiceClient();
        }

        private DataHealthApiServiceClient GetDEHServiceClient()
        {
            return this.apiServiceClientFactory.GetClient();
        }

        public async Task TriggerMDQJobCallback(MDQJobCallbackPayload payload)
        {
            this.logger.LogInformation($"Start to trigger MDQ Job callback. Job Id: {payload.DQJobId}. Job status: {payload.JobStatus}.");
            var client = this.GetDEHServiceClient();
            try
            {
                await client.TriggerMDQJobCallback(payload).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Fail to trigger MDQ Job callback", ex);
                throw;
            }
        }
    }
}