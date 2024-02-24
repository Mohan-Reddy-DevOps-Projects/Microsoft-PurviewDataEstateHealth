// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Rest;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

using System.Text.Json;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

/// <summary>
/// Http Client handler
/// </summary>
public abstract class HttpClientBase<TBase> : ServiceClient<TBase>
    where TBase : ServiceClient<TBase>
{
    /// <summary>
    /// The api-version.
    /// </summary>
    public string ApiVersion { get; set; }

    /// <summary>
    /// The base URI.
    /// </summary>
    public Uri BaseUri { get; set; }

    /// <summary>
    /// The json serializer options.
    /// </summary>
    protected static readonly JsonSerializerOptions options = new()
    {
        Converters = {
            new JsonStringEnumConverter(),
        },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// The service logger.
    /// </summary>
    protected readonly IDataEstateHealthRequestLogger logger;

    /// <summary>
    /// The client name used to connect to the service.
    /// </summary>
    protected readonly string clientName;

    /// <summary>
    /// The base http client.
    /// </summary>
    /// <param name="logger">the logger.</param>
    /// <param name="httpClient">the client itself.</param>
    /// <param name="clientName">the name of the client.</param>
    /// <param name="disposeHttpClient">should we dispose?</param>
    public HttpClientBase(
        IDataEstateHealthRequestLogger logger,
        HttpClient httpClient,
        string clientName,
        bool disposeHttpClient) : base(httpClient, disposeHttpClient)
    {
        this.logger = logger;
        this.clientName = clientName;
    }

    /// <summary>
    /// Get request
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="headers"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<HttpResponseMessage> GetAsync(
        string endpoint,
        IList<KeyValuePair<string, string>> headers,
        CancellationToken cancellationToken)
    {
        return await this.SendAsync<object>(this.ConstructEndpoint(endpoint), HttpMethod.Get, headers, cancellationToken);
    }

    /// <summary>
    /// Post request
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="endpoint"></param>
    /// <param name="body"></param>
    /// <param name="headers"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<HttpResponseMessage> PostAsync<TRequest>(
        string endpoint,
        TRequest body,
        IList<KeyValuePair<string, string>> headers,
        CancellationToken cancellationToken) where TRequest : class
    {
        return await this.SendAsync(this.ConstructEndpoint(endpoint), HttpMethod.Post, headers, cancellationToken, body);
    }

    /// <summary>
    /// Delete request
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="headers"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<HttpResponseMessage> DeleteAsync(
        string endpoint,
        IList<KeyValuePair<string, string>> headers,
        CancellationToken cancellationToken)
    {
        return await this.SendAsync<object>(this.ConstructEndpoint(endpoint), HttpMethod.Delete, headers, cancellationToken);
    }

    /// <summary>
    /// Reads the response stream into the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected static async Task<T> DeserializeResponse<T>(Stream stream, JsonSerializerOptions options, CancellationToken cancellationToken)
    {
        MemoryStream ms = new();
        await stream.CopyToAsync(ms, cancellationToken);
        ms.Seek(0, SeekOrigin.Begin);

        return JsonSerializer.Deserialize<T>(ms, options);
    }

    /// <summary>
    /// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    protected static async Task<string> ResponseToString(Stream stream)
    {
        using StreamReader reader = new(stream);

        return await reader.ReadToEndAsync();
    }

    private async Task<HttpResponseMessage> SendAsync<TRequest>(
        Uri endpoint,
        HttpMethod method,
        IList<KeyValuePair<string, string>> headers,
        CancellationToken cancellationToken,
        TRequest body = null) where TRequest : class
    {
        this.logger.LogInformation($"Client: {this.clientName} Method: {method} Endpoint: {endpoint.AbsoluteUri}");
        using HttpRequestMessage request = await CreateRequest(endpoint, method, body, headers, cancellationToken);

        return await this.HttpClient.SendAsync(request, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(Uri endpoint, HttpMethod method, IList<KeyValuePair<string, string>> headers)
    {
        var request = new HttpRequestMessage(method, endpoint);
        if (headers?.Any() ?? false)
        {
            foreach (KeyValuePair<string, string> kvp in headers)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }
        }

        return request;
    }

    private static async Task<HttpRequestMessage> CreateRequest<T>(Uri endpoint, HttpMethod method, T body, IList<KeyValuePair<string, string>> headers, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = CreateRequest(endpoint, method, headers);

        if (body is HttpContent httpContent)
        {
            request.Content = httpContent;
        }
        else if (body != null)
        {
            MemoryStream ms = new();
            await JsonSerializer.SerializeAsync(ms, body, options, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var requestContent = new StreamContent(ms);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = requestContent;
        }

        return request;
    }

    /// <summary>
    /// Constructs a URI frm the base URI and the request path.
    /// </summary>
    /// <param name="path">The path of the endpoint. Must not include a leading forward slash.</param>
    /// <returns></returns>
    private Uri ConstructEndpoint(string path)
    {
        return new UriBuilder($"{this.BaseUri}{path}").Uri;
    }
}
