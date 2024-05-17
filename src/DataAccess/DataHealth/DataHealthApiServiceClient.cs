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

        public async Task TriggerScheduleCallback(TriggeredSchedulePayload payload, CancellationToken cancellationToken)
        {
            var requestUri = this.CreateRequestUri("/internal/control/triggerScheduleJobCallback");
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
