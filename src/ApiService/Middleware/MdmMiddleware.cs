namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using System.Diagnostics;

internal sealed class MdmMiddleware
{
    private const string Tag = "MdmMiddleware";
    private readonly RequestDelegate next;

    public MdmMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// Invoke the middleware.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public async Task Invoke(
        HttpContext httpContext,
        IDataEstateHealthRequestLogger logger)
    {
        try
        {
            var routeData = httpContext.GetRouteData();
            string action = HttpRequestLatencyEvent.TryFetchRouteData(routeData, HttpRequestLatencyEvent.action);
            string controller = HttpRequestLatencyEvent.TryFetchRouteData(routeData, HttpRequestLatencyEvent.controller);

            logger.LogInformation($"Incoming request - HttpMethod: {httpContext.Request.Method}; Action: {action}; Controller: {controller}");
            await this.HandleRequest(httpContext, logger);
        }
        catch (Exception ex)
        {
            logger.LogError($"{Tag}| Unhandled exception occurred.", ex);
            throw;
        }
    }

    private async Task HandleRequest(
        HttpContext httpContext,
        IDataEstateHealthRequestLogger logger)
    {
        int statusCode = httpContext.Response.StatusCode;
        Stopwatch timer = Stopwatch.StartNew();
        try
        {
            await this.next(httpContext);
            timer.Stop();
            statusCode = httpContext.Response.StatusCode;
        }
        catch (Exception ex)
        {
            timer.Stop();
            statusCode = (int)ExceptionConverter.GetHttpStatusCode(ex);
            throw;
        }
        finally
        {
            HttpRequestLatencyEvent requestLatencyEvent = HttpRequestLatencyEvent.GenerateRequestLatencyEvent(statusCode, httpContext);
            logger.LogInformation($"End User Request. HttpMethod: [{requestLatencyEvent.HttpMethod}] Controller: {requestLatencyEvent.Controller} Action: {requestLatencyEvent.Action} StatusCode: {statusCode} ElapsedMs: {timer.ElapsedMilliseconds}");
        }
    }
}
