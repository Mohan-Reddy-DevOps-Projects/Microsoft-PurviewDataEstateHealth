// <copyright file="PurviewMITokenClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.StorageConfig
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class MSITokenResponse
    {
        [JsonProperty("access_token")]
        public required string AccessToken { get; set; }

        [JsonProperty("expires_on")]

        public required long ExpiresOn { get; set; }

        [JsonProperty("resource")]

        public required string Resource { get; set; }

        [JsonProperty("token_type")]

        public required string TokenType { get; set; }
    }

    public class PurviewMITokenClient : ServiceClient<PurviewMITokenClient>
    {
        private readonly Uri BaseUri;

        private readonly IDataEstateHealthRequestLogger Logger;

        private readonly HttpClient Client;

        public PurviewMITokenClient(HttpClient httpClient, Uri baseUri, IDataEstateHealthRequestLogger logger)
        {
            this.Client = httpClient;
            this.BaseUri = baseUri;
            this.Logger = logger;
        }

        public async Task<string> GetMIToken(string accountId, string resourcetype)
        {
            var requestUri = new UriBuilder(this.BaseUri)
            {
                Path = $"/api/v1/accounts/{accountId}/tokens/byocmitoken",
                Query = $"resourcetype={resourcetype}"
            }.Uri;
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("x-ms-account-id", accountId);
            var response = await this.Client.SendAsync(request).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
            var content = response.Content.ReadAsAsync<string>().Result;
            var token = JsonConvert.DeserializeObject<MSITokenResponse>(content);
            return token?.AccessToken ?? throw new Exception("Failed to deserialize MI token");
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
