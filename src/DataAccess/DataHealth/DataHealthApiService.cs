﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
    using Microsoft.Azure.ProjectBabylon.Metadata.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.JobManagerModels;
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
            var client = this.apiServiceClientFactory.GetClient();
            client.HttpClient.Timeout = TimeSpan.FromMinutes(5);
            return client;
        }

        /// <summary>
        /// Generate Purview MI token, using DEH -> RP services 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task<string> GetMIToken(string accountId)
        {
            string returnToken = "";
            var client = this.GetDEHServiceClient();

            var resonseString = await client.GetMITokenfromDEH(accountId, CancellationToken.None);
            //await this.purviewMITokenClient.GetMIToken(accountId, "azurestorage").ConfigureAwait(false);

            var payload = JsonConvert.DeserializeObject<MITokenPayload>(resonseString);
            returnToken = payload.Token;
            if (string.IsNullOrEmpty(returnToken))
            {
                this.logger.LogInformation($"Get MI token from DEH. Account Id: {accountId}, empty token generated, MI Token generation failed.");
            }

            return returnToken;
        }


        public async Task<string> GetDEHSKUConfig(string accountId)
        {
            string returnSKU = "basic";
            try
            {
                var client = this.GetDEHServiceClient();
                var responseString = await client.GetDEHSKUConfig(accountId, CancellationToken.None);
                this.logger.LogInformation($"GetDEHSKUConfig from Catalog By Account Id: {accountId}. Response: {responseString}");

                var payload = JsonConvert.DeserializeObject<CatalogDEHSKUSettingsModel>(responseString);
                if (payload.Sku != null)
                {
                    returnSKU = payload.Sku;
                    if (string.IsNullOrEmpty(returnSKU))
                    {
                        this.logger.LogInformation($"GetDEHSKUConfig from Catalog. Account Id: {accountId} failed!");
                    }
                }
                else
                {
                    this.logger.LogInformation($"GetDEHSKUConfig from Catalog. Account Id: {accountId} failed!");
                    return null;
                }
                return returnSKU;

            }
            catch (Exception ex)
            {
                this.logger.LogWarning($"GetDEHSKUConfig|Fail to get DEH SKU Config: {accountId}.", ex);
            }
            return returnSKU;
        }

        /// <summary>
        /// Get Storage Config settings from DEH, the User will configure this from DEH settings
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<StorageConfiguration> GetStorageConfigSettings(string accountId, string tenantId)
        {
            string returnLocationURL = "";
            try
            {
                var client = this.GetDEHServiceClient();
                var responseString = await client.GetStorageConfigSettings(accountId, tenantId, CancellationToken.None);
                this.logger.LogInformation($"GetStorageConfigSettings from DEH. Account Id: {accountId}. Response: {responseString}");

                var payload = JsonConvert.DeserializeObject<StorageConfiguration>(responseString);
                if (payload.TypeProperties != null)
                {
                    returnLocationURL = payload.TypeProperties.LocationURL;
                    if (string.IsNullOrEmpty(returnLocationURL))
                    {
                        this.logger.LogInformation($"GetStorageConfigSettings from DEH. Account Id: {accountId} failed!");
                    }
                }
                else
                {
                    this.logger.LogInformation($"GetStorageConfigSettings from DEH. Account Id: {accountId} failed!");
                    return null;
                }
                return payload;

            }
            catch (Exception ex)
            {
                this.logger.LogError($"GetStorageConfigSettings|Fail to get self serve settings for storage. Job Id: {accountId}.", ex);
            }
            return null;
        }

        public void TriggerMDQJobCallback(MDQJobModel jobModel, bool isRetry, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            this.logger.LogInformation($"Start to trigger MDQ Job callback. Job Id: {jobModel.DQJobId}. Job status: {jobModel.JobStatus}. Request ID: {requestId}. Job retry: {isRetry}.");
            Task.Run(async () =>
            {
                try
                {

                    if (isRetry && jobModel.CreatedAt < DateTimeOffset.Now.AddDays(-5))
                    {
                        this.logger.LogInformation($"Clean outdated failed MDQ job. Job Id: {jobModel.DQJobId}. Job status: {jobModel.JobStatus}. Request ID: {requestId}.");
                        await this.CleanMDQFailedJob(jobModel.DQJobId).ConfigureAwait(false);
                        return;
                    }

                    var payload = new MDQJobCallbackPayload
                    {
                        DQJobId = jobModel.DQJobId,
                        JobStatus = jobModel.JobStatus,
                        TenantId = jobModel.TenantId,
                        AccountId = jobModel.AccountId,
                        IsRetry = isRetry,
                        RequestId = requestId
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

                    if (isRetry && jobModel.CreatedAt < DateTimeOffset.Now.AddDays(-2))
                    {
                        this.logger.LogCritical($"Failed MDQ job retried for more than two days. Error: {ex.Message}. Job Id: {jobModel.DQJobId}. Job status: {jobModel.JobStatus}. Request ID: {requestId}.");
                    }
                }
            });
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

        public async Task<CreateDataQualityRulesSpecResult> CreateDataQualityRulesSpec(TriggeredSchedulePayload payload, 
            CancellationToken cancellationToken)
        {
            string payloadString = JsonConvert.SerializeObject(payload);
            using (this.logger.LogElapsed($"Create data quality rules specification. {payloadString}"))
            {
                try
                {
                    var client = this.GetDEHServiceClient();
                    string controlsWorkflowId = await client.CreateDataQualitySpec(payload, cancellationToken)
                        .ConfigureAwait(false);
                    this.logger.LogInformation($"Succeed to create data quality rules specification. " +
                                               $"{payloadString}, SpecificationId: {controlsWorkflowId}");
                    return new CreateDataQualityRulesSpecResult(true, controlsWorkflowId);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Fail to create data quality rules specification. {payloadString}", ex);
                    return new CreateDataQualityRulesSpecResult(false, null);
                }
            }
        }

        public async Task TriggerActionsUpsert(MDQJobModel jobModel, string traceId, CancellationToken cancellationToken)
        {
            var requestId = new Guid(traceId);
            this.logger.LogInformation($"Start to trigger actions creation for control: {jobModel.ControlId}, DQ Job Id: {jobModel.DQJobId}, Job status: {jobModel.JobStatus}, Request ID: {requestId}");
            
            var payload = new UpsertMdqActionsPayload
            {
                DQJobId = jobModel.DQJobId,
                JobStatus = jobModel.JobStatus,
                TenantId = jobModel.TenantId,
                AccountId = jobModel.AccountId,
                IsRetry = false,
                RequestId = requestId,
                ControlId = jobModel.ControlId
            };
            var client = this.GetDEHServiceClient();
            await client.UpsertMdqActions(payload, cancellationToken).ConfigureAwait(false);
            this.logger.LogInformation($"Succeed to trigger actions creation for control: {jobModel.ControlId}, DQ Job Id: {jobModel.DQJobId}.");
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