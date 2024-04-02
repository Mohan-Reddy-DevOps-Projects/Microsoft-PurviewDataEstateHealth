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

        public async Task TriggerMDQJobCallback(MDQJobCallbackPayload schedule)
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
            var response = await this.Client.SendAsync(request).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
        }

        public async Task CleanUpActionJobCallback(AccountServiceModel account)
        {
            var requestUri = this.CreateRequestUri("/internal/actions/cleanup");
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);

            request.Headers.Add(HeaderAccountIdName, account.Id.ToString());
            request.Headers.Add(HeaderTenantIdName, account.TenantId.ToString());
            var response = await this.Client.SendAsync(request).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
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

        private void HandleResponseStatusCode(HttpResponseMessage response)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                this.Logger.LogError("Data health api service request failed", ex);
                throw;
            }
        }
    }
}
