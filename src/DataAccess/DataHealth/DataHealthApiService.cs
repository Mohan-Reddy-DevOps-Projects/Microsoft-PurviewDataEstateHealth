// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
    using Microsoft.Azure.ProjectBabylon.Metadata.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Newtonsoft.Json;
    using System;
    using System.Threading;
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

        public void TriggerMDQJobCallback(MDQJobModel jobModel, bool isRetry, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            this.logger.LogInformation($"Start to trigger MDQ Job callback. Job Id: {jobModel.DQJobId}. Job status: {jobModel.JobStatus}. Request ID: {requestId}. Job retry: {isRetry}.");
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
                        IsRetry = isRetry,
                        RequestId = requestId,
                    };
                    var client = this.GetDEHServiceClient();
                    await client.TriggerMDQJobCallback(payload, cancellationToken).ConfigureAwait(false);
                    this.logger.LogInformation($"Succeed to trigger MDQ Job callback. Job Id: {jobModel.DQJobId}.");

                    if (isRetry)
                    {
                        await this.CleanMDQFailedJob(jobModel.DQJobId).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Fail to trigger MDQ Job callback. Job Id: {jobModel.DQJobId}.", ex);
                    await this.CreateMDQFailedJob(jobModel).ConfigureAwait(false);

                    if (isRetry && ex.Message.Contains("Fail to get computing job by DQ job id"))
                    {
                        await this.CleanMDQFailedJob(jobModel.DQJobId).ConfigureAwait(false);
                        this.logger.LogError($"Monitoring job not found failed job removed. DQ job id: {jobModel.DQJobId}.");
                    }
                }
            });
        }

        public async Task<bool> TriggerDEHScheduleCallback(TriggeredSchedulePayload payload, CancellationToken cancellationToken)
        {
            var payloadString = JsonConvert.SerializeObject(payload);
            using (this.logger.LogElapsed($"Trigger DEH schedule callback. {payloadString}"))
            {
                try
                {
                    var client = this.GetDEHServiceClient();
                    await client.TriggerScheduleCallback(payload, cancellationToken).ConfigureAwait(false);
                    this.logger.LogInformation($"Succeed to trigger DEH schedule callback. {payloadString}");
                    return true;
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Fail to trigger DEH schedule callback. {payloadString}", ex);
                    return false;
                }
            }
        }

        public async Task<bool> TriggerDEHSchedule(TriggeredSchedulePayload payload, CancellationToken cancellationToken)
        {
            var payloadString = JsonConvert.SerializeObject(payload);
            using (this.logger.LogElapsed($"Trigger DEH schedule. {payloadString}"))
            {
                try
                {
                    var client = this.GetDEHServiceClient();
                    await client.TriggerSchedule(payload, cancellationToken).ConfigureAwait(false);
                    this.logger.LogInformation($"Succeed to trigger DEH schedule. {payloadString}");
                    return true;
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Fail to trigger DEH schedule. {payloadString}", ex);
                    return false;
                }
            }
        }

        public async Task<bool> CleanUpActionsJobCallback(AccountServiceModel account, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Start to clean up actions callback.");
            try
            {
                var client = this.GetDEHServiceClient();
                await client.CleanUpActionJobCallback(account, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation($"Succeed to clean up actions. Tenant Id: {account.TenantId}.");
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Fail to clean up actions. Tenant Id: {account.TenantId}.", ex);
                return false;
            }
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
                    model.RetryCount += 1;
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