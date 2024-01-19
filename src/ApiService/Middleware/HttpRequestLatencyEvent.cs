namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.Globalization;

/// <summary>
/// Http outgoing request latency event class
/// </summary>
internal sealed class HttpRequestLatencyEvent
{
    public const string action = "action";
    public const string controller = "controller";

    /// <summary>
    /// Request action
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// Request controller
    /// </summary>
    public string Controller { get; set; }

    /// <summary>
    /// the HttpMethod property
    /// </summary>
    public string HttpMethod { get; set; }

    /// <summary>
    /// the StatusCode property
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// the request endpoint
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Create a request latency event based on Http context and configuration
    /// </summary>
    /// <param name="statusCode">The status code to be reported to the caller. Http context status code cannot be trusted until the exception handler processes the event.</param>
    /// <param name="httpContext">Http context.</param>
    /// <returns>
    /// An HttpRequestLatencyEvent object
    /// </returns>
    public static HttpRequestLatencyEvent GenerateRequestLatencyEvent(int statusCode, HttpContext httpContext)
    {
        RouteData routeData = httpContext.GetRouteData();

        return new HttpRequestLatencyEvent()
        {
            Action = TryFetchRouteData(routeData, action).Replace(Environment.NewLine, string.Empty),
            Controller = TryFetchRouteData(routeData, controller).Replace(Environment.NewLine, string.Empty),
            HttpMethod = httpContext.Request?.Method,
            StatusCode = statusCode.ToString(CultureInfo.InvariantCulture),
            Endpoint = httpContext.Request?.Path,
        };
    }

    /// <summary>
    /// Fetch route data. Return empty string if it does not contain the key.
    /// </summary>
    /// <param name="routeData">the route of the request</param>
    /// <param name="key">the key to look up from the route</param>
    public static string TryFetchRouteData(RouteData routeData, string key)
    {
        return routeData != null && routeData.Values.ContainsKey(key) ? routeData.Values[key].ToString() : string.Empty;
    }
}