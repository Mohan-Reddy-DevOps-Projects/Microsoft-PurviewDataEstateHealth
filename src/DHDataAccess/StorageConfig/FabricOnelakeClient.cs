// <copyright file="FabricOnelakeClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.StorageConfig
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    public class FabricOnelakeClient : ServiceClient<FabricOnelakeClient>
    {
        private readonly Uri BaseUri = new UriBuilder("https://onelake.dfs.fabric.microsoft.com").Uri;

        private readonly IDataEstateHealthRequestLogger Logger;

        private readonly HttpClient Client;

        public FabricOnelakeClient(HttpClient httpClient, IDataEstateHealthRequestLogger logger)
        {
            this.Client = httpClient;
            this.Logger = logger;
        }

        public async Task CreateFile(string filename, string token)
        {
            var requestUri = this.CreateRequestUri(filename, "resource=file");
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await this.Client.SendAsync(request).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
        }

        public async Task DeleteFile(string filename, string token)
        {
            var requestUri = this.CreateRequestUri(filename);
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await this.Client.SendAsync(request).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
        }

        private Uri CreateRequestUri(string pathname, string? query = null)
        {
            var builder = new UriBuilder(this.BaseUri)
            {
                Path = pathname,
            };
            if (query != null)
            {
                builder.Query = query;
            }
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
                this.Logger.LogError("Schedule service request failed", ex);
                throw;
            }
        }
    }
}
