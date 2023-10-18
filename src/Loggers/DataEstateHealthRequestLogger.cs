// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Logging;

/// <inheritdoc cref="DataEstateHealthLogger" />
/// <inheritdoc cref="IDataEstateHealthRequestLogger" />
/// <summary>
/// Scoped logger implementation class.
/// </summary>
internal class DataEstateHealthRequestLogger : DataEstateHealthLogger, IDataEstateHealthRequestLogger
{
    private readonly IRequestHeaderContext requestHeaderContext;

    /// <inheritdoc />
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Microsoft.Azure.PurviewShare.Logger.purviewShareRequestLogger" /> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use</param>
    /// <param name="requestHeaderContext">The correlation context.</param>
    public DataEstateHealthRequestLogger(
        ILoggerFactory loggerFactory,
        IRequestHeaderContext requestHeaderContext)
        : base(loggerFactory)
    {
        this.requestHeaderContext = requestHeaderContext;
    }

    /// <inheritdoc />
    protected override IRequestHeaderContext GetRequestHeaderContext()
    {
        return this.requestHeaderContext;
    }
}
