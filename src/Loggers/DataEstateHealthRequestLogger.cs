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
    private readonly IRequestContextAccessor requestContextAccessor;

    /// <inheritdoc />
    /// <summary>
    /// Initializes a new instance of the <see cref="DataEstateHealthRequestLogger" /> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use</param>
    /// <param name="requestContextAccessor">The correlation context.</param>
    public DataEstateHealthRequestLogger(
        ILoggerFactory loggerFactory,
        IRequestContextAccessor requestContextAccessor)
        : base(loggerFactory)
    {
        this.requestContextAccessor = requestContextAccessor;
    }

    /// <inheritdoc />
    protected override IRequestHeaderContext GetRequestHeaderContext()
    {
        return this.requestContextAccessor?.GetRequestContext();
    }
}
