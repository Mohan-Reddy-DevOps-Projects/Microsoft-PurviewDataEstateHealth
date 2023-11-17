// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Core;
using global::Azure.Storage.Files.DataLake;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;

/// <summary>
/// DeltaLakeOperatorFactory
/// </summary>
public class DeltaLakeOperatorFactory : IDeltaLakeOperatorFactory
{
    private readonly EnvironmentConfiguration environmentConfig;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    /// <summary>
    /// DeltaLakeOperatorFactory C'tor
    /// </summary>
    /// <param name="environmentConfig"></param>
    /// <param name="dataEstateHealthRequestLogger"></param>
    public DeltaLakeOperatorFactory(IOptions<EnvironmentConfiguration> environmentConfig, IDataEstateHealthRequestLogger dataEstateHealthRequestLogger)
    {
        this.environmentConfig = environmentConfig.Value;
        this.dataEstateHealthRequestLogger = dataEstateHealthRequestLogger;
    }

    /// <inheritdoc />
    public IDeltaLakeOperator Build(Uri adlsGen2FileSystemEndpoint, TokenCredential tokenCredential)
    {
        DataLakeFileSystemClient adlsGen2ServiceClient = new DataLakeFileSystemClient(adlsGen2FileSystemEndpoint, tokenCredential);
        return new DeltaLakeOperator(adlsGen2ServiceClient, environmentConfig, dataEstateHealthRequestLogger);
    }
}
