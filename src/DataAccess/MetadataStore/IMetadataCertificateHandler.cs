// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Interface to create handler for babylon metadata client.
/// </summary>
internal interface IMetadataCertificateHandler
{
    /// <summary>
    /// Method to create http handler for babylon metadata client.
    /// </summary>
    /// <returns></returns>
    Task<SocketsHttpHandler> CreateHandlerAsync();
}
