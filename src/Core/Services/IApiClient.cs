// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

internal interface IApiClient
{
    /// <summary>
    /// HTTP Get Operation
    /// </summary>
    /// <param name="httpRequestMessage">The http request message.</param>
    /// <param name="endPointType">The end point type</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>http response message</returns>
    Task<HttpResponseMessage> GetAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// HTTP Put Operation
    /// </summary>
    /// <param name="httpRequestMessage">The http request message.</param>
    /// <param name="endPointType">The end point type</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>http response message</returns>
    Task<HttpResponseMessage> PutAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// HTTP Post Operation
    /// </summary>
    /// <param name="httpRequestMessage">The http request message.</param>
    /// <param name="endPointType">The end point type</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>http response message</returns>
    Task<HttpResponseMessage> PostAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// HTTP Delete Operation
    /// </summary>
    /// <param name="httpRequestMessage">The http request message.</param>
    /// <param name="endPointType">The end point type</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>http response message</returns>
    Task<HttpResponseMessage> DeleteAsync(
        HttpRequestMessage httpRequestMessage,
        EndPointType endPointType,
        CancellationToken cancellationToken = default);
}
