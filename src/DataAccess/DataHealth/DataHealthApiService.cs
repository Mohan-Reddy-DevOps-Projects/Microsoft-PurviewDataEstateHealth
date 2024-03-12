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
        private readonly IMDQFailedJobRepository mdqFailedJobRepsository;

        public DataHealthApiService(
            DataHealthApiServiceClientFactory apiServiceClientFactory,
            IDataEstateHealthRequestLogger logger,
            IMDQFailedJobRepository mdqFailedJobRepsository)
        {
            this.apiServiceClientFactory = apiServiceClientFactory;
            this.logger = logger;
            this.mdqFailedJobRepsository = mdqFailedJobRepsository;
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

        public void TriggerMDQJobCallback(MDQJobModel jobModel, bool isRetry)
        {
            this.logger.LogInformation($"Start to trigger MDQ Job callback. Job Id: {jobModel.DQJobId}. Job status: {jobModel.JobStatus}. Job retry: {isRetry}.");
            Task.Run(async () =>
            {
                try
                {
                    var payload = new MDQJobCallbackPayload
                    {
                        DQJobId = jobModel.DQJobId,
                        JobStatus = jobModel.JobStatus,
                        TenantId = jobModel.TenantId,
                        AccountId = jobModel.AccountId,
                        IsRetry = isRetry
                    };
                    var client = this.GetDEHServiceClient();
                    await client.TriggerMDQJobCallback(payload).ConfigureAwait(false);

                    if (isRetry)
                    {
                        await this.CleanMDQFailedJob(jobModel.DQJobId).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError("Fail to trigger MDQ Job callback", ex);
                    await this.CreateMDQFailedJob(jobModel).ConfigureAwait(false);
                }
            });
        }

        public async Task CreateMDQFailedJob(MDQJobModel model)
        {
            try
            {
                var currentModel = await this.mdqFailedJobRepsository.GetSingle(model.DQJobId, CancellationToken.None).ConfigureAwait(false);
                if (currentModel == null)
                {
                    await this.mdqFailedJobRepsository.Create(model, CancellationToken.None);
                }
                else
                {
                    await this.mdqFailedJobRepsository.Update(model, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred during creating MDQ failed job", ex);
            }
        }

        public async Task CleanMDQFailedJob(Guid eventId)
        {
            try
            {
                var model = await this.mdqFailedJobRepsository.GetSingle(eventId, CancellationToken.None).ConfigureAwait(false);
                if (model != null)
                {
                    await this.mdqFailedJobRepsository.Delete(eventId, CancellationToken.None).ConfigureAwait(false);
                    this.logger.LogInformation($"Delete MDQ failed job in table storage. Job Id: {model.DQJobId}. Account Id: {model.AccountId}. Tenant Id: {model.TenantId}.");
                }
                else
                {
                    this.logger.LogInformation($"Not found MDQ failed job {eventId}. Ignore clean.");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred during clean MDQ failed job", ex);
            }
        }
    }
}