// <copyright file="DataHealthApiServiceClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
    using Microsoft.Azure.ProjectBabylon.Metadata.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.Common;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class DataHealthApiServiceClient : ServiceClient<DataHealthApiServiceClient>
    {
        private const string HeaderAccountIdName = "x-ms-account-id";
        private const string HeaderTenantIdName = "x-ms-client-tenant-id";
        private const string HeaderRequestIdName = "x-ms-client-request-id";
        private readonly Uri BaseUri;        

        private readonly IDataEstateHealthRequestLogger Logger;

        private readonly HttpClient Client;

        private const string ApiVersion = ServiceVersion.LabelV2;
        private const string ApiVersionv1 = ServiceVersion.LabelV1;

        public DataHealthApiServiceClient(HttpClient httpClient, Uri baseUri, IDataEstateHealthRequestLogger logger)
        {
            this.Client = httpClient;
            this.BaseUri = baseUri;            
            this.Logger = logger;
        }

        public async Task TriggerMDQJobCallback(MDQJobCallbackPayload schedule, CancellationToken cancellationToken)
        {
            var requestUri = this.CreateRequestUri("/internal/control/triggerMDQJobCallback");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = this.CreateRequestContent(new Dictionary<string, string>()
            {
                ["dqJobId"] = schedule.DQJobId.ToString(),
                ["jobStatus"] = schedule.JobStatus
            });
            request.Headers.Add(HeaderAccountIdName, schedule.AccountId.ToString());
            request.Headers.Add(HeaderTenantIdName, schedule.TenantId.ToString());
            request.Headers.Add(HeaderRequestIdName, schedule.RequestId.ToString());
            var response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            await this.HandleResponseStatusCode(response);
        }

        public async Task UpsertMdqActions(UpsertMdqActionsPayload payload, CancellationToken cancellationToken)
        {
            var requestUri = this.CreateRequestUri("/internal/control/upsertMdqActions");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = this.CreateRequestContent(new Dictionary<string, string>()
            {
                ["dqJobId"] = payload.DQJobId.ToString(),
                ["jobStatus"] = payload.JobStatus,
                ["controlId"] = payload.ControlId
            });
            request.Headers.Add(HeaderAccountIdName, payload.AccountId.ToString());
            request.Headers.Add(HeaderTenantIdName, payload.TenantId.ToString());
            request.Headers.Add(HeaderRequestIdName, payload.RequestId.ToString());
            var response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            await this.HandleResponseStatusCode(response);
        }

        public async Task<string> GetMITokenfromDEH(string accountId, CancellationToken cancellationToken)
        {
            var requestUri = this.CreateRequestUri($"/internal/settings/storageConfig/mitoken");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add(HeaderAccountIdName, accountId);
            var response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //await this.HandleResponseStatusCode(response);
            return responseContent;
        }

        public async Task<string> GetStorageConfigSettings(string accountId, string tenantId, CancellationToken cancellationToken)
        {
            var requestUri = this.CreateRequestUri($"/internal/settings/storageConfig");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add(HeaderAccountIdName, accountId);
            request.Headers.Add(HeaderTenantIdName, tenantId);
            var response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //await this.HandleResponseStatusCode(response);
            return responseContent;
        }


        public async Task<string> GetDEHSKUConfig(string accountId, CancellationToken cancellationToken)
        {
            var requestUri = this.CreateProvisioningRequestUri($"/config");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add(HeaderAccountIdName, accountId);
            request.Headers.Add(HeaderTenantIdName, "tenantId");
            var response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //await this.HandleResponseStatusCode(response);
            return responseContent;
        }

        public async Task TriggerSchedule(TriggeredSchedulePayload payload, CancellationToken cancellationToken)
        {
            var requestUri = this.CreateRequestUri("/internal/control/triggerScheduleJob");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = this.CreateRequestContent(new Dictionary<string, string>()
            {
                ["controlId"] = payload.ControlId,
                ["operator"] = payload.Operator,
                ["triggerType"] = payload.TriggerType,
            });
            request.Headers.Add(HeaderAccountIdName, payload.AccountId.ToString());
            request.Headers.Add(HeaderTenantIdName, payload.TenantId.ToString());
            request.Headers.Add(HeaderRequestIdName, payload.RequestId.ToString());
            var response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            await this.HandleResponseStatusCode(response);
        }

        public async Task<string> CreateDataQualitySpec(TriggeredSchedulePayload payload, 
            CancellationToken cancellationToken)
        {
            var requestUri = this.CreateRequestUri("/internal/control/createDataQualitySpec");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = this.CreateRequestContent(new Dictionary<string, string>()
            {
                ["controlId"] = payload.ControlId,
                ["operator"] = payload.Operator,
                ["triggerType"] = payload.TriggerType,
            });
            request.Headers.Add(HeaderAccountIdName, payload.AccountId.ToString());
            request.Headers.Add(HeaderTenantIdName, payload.TenantId.ToString());
            request.Headers.Add(HeaderRequestIdName, payload.RequestId.ToString());
            var response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            await this.HandleResponseStatusCode(response);
            
            // Read and return the job run ID from the response
            string jobRunId = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return jobRunId?.Trim('"'); // Remove quotes if the response is a JSON string
        }

        public async Task CleanUpActionJobCallback(AccountServiceModel account, CancellationToken cancellationToken)
        {
            var requestUri = this.CreateRequestUri("/internal/actions/cleanup");
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);

            request.Headers.Add(HeaderAccountIdName, account.Id.ToString());
            request.Headers.Add(HeaderTenantIdName, account.TenantId.ToString());
            var response = await this.Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            await this.HandleResponseStatusCode(response);
        }

        private Uri CreateRequestUri(string pathname)
        {
            var builder = new UriBuilder(this.BaseUri)
            {
                Path = pathname,
                Query = $"api-version={ApiVersion}"
            };
            return builder.Uri;
        }


        private Uri CreateProvisioningRequestUri(string pathname)
        {
            //https://df-westus2-prov.purview-dg.azure-test.com/config?api-version=2023-10-01-preview
            var builder = new UriBuilder(this.BaseUri.ToString().Replace("health", "prov"))
            {
                Path = pathname,
                Query = $"api-version={ApiVersionv1}"
            };
            return builder.Uri;
        }


        private HttpContent CreateRequestContent(object obj)
        {
            var content = JsonConvert.SerializeObject(obj);
            return new StringContent(content, Encoding.UTF8, "application/json");
        }

        private async Task HandleResponseStatusCode(HttpResponseMessage response)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var errorMessage = $"Data health api service request failed. {response.StatusCode} {responseContent}";
                this.Logger.LogError(errorMessage, ex);
                throw new Exception(errorMessage, ex);
            }
        }
    }
}
