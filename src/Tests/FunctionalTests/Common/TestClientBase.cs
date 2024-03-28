namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService;
using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class TestClientBase
{
    private const string ApiVersion = "2024-02-01-preview";
    private HttpClient Client;
    private TestConfiguration Configurations = new();

    public void InitializeClient()
    {
        this.Configurations = TestConfigurationHelper.GetApplicationConfiguration();
        this.Client = this.GetHttpClient();
    }

    public async Task<T> Request<T>(
       string url,
       HttpMethod method,
       object payload = null)
    {
        var payloadContent = payload == null ? null : JsonConvert.SerializeObject(payload);
        var response = await this.RequestAndGetRawResponse(url, method, payloadContent).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        Assert.IsTrue(response.IsSuccessStatusCode, $"Response fail with status code {response.StatusCode}. Content:\n{content}.");
        return JsonConvert.DeserializeObject<T>(content);
    }

    public async Task<HttpResponseMessage> RequestAndGetRawResponse(
        string url,
        HttpMethod method,
        string payloadContent = null)
    {
        HttpResponseMessage response = null;
        if (method == null)
        {
            throw new ArgumentNullException(nameof(method));
        }
        using (var payload = payloadContent != null ? new StringContent(payloadContent, Encoding.UTF8, "application/json") : null)
        {
            url += url.Contains('?') ? "&" : "?";
            url += $"api-version={ApiVersion}";
            using HttpRequestMessage request = new(method, url);
            switch (method.Method)
            {
                case "GET":
                    break;
                case "POST":
                    request.Content = payload;
                    break;
                case "PUT":
                    request.Content = payload;
                    break;
                case "DELETE":
                    break;
                default:
                    throw new NotSupportedException($"Unsupported HTTP method: {method.Method}");
            }
            response = await this.Client.SendAsync(request).ConfigureAwait(false);
        }
        return response;
    }

    private HttpClient GetHttpClient()
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var webAppFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(Environments.Development);
        });

#pragma warning restore CA2000 // Dispose objects before losing scope
        var httpClient = webAppFactory.CreateDefaultClient();

        httpClient.DefaultRequestHeaders.Add("x-ms-account-id", this.Configurations.AccountId);
        httpClient.DefaultRequestHeaders.Add("x-ms-account-name", this.Configurations.AccountName);
        httpClient.DefaultRequestHeaders.Add("x-ms-client-tenant-id", this.Configurations.TenantId);
        httpClient.DefaultRequestHeaders.Add("x-ms-client-name", this.Configurations.AccountName);
        return httpClient;
    }
}
