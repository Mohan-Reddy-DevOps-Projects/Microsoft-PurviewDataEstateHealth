// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;

/// <summary>
/// Spark pool config properties.
/// </summary>
public class SparkPoolConfigProperties
{
    /// <summary>
    /// Gets or sets the pool size.
    /// </summary>
    public string PoolSize { get; set; }

    /// <summary>
    /// Gets or sets the driver memory size.
    /// </summary>
    public string DriverMemorySize { get; set; }

    /// <summary>
    /// Gets or sets the executor cores.
    /// </summary>
    public int ExecutorCores { get; set; }

    /// <summary>
    /// Gets or sets the driver cores.
    /// </summary>
    public int DriverCores { get; set; }

    /// <summary>
    /// Gets or sets the executor memory size.
    /// </summary>
    public string ExecutorMemorySize { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkPoolConfigProperties"/> class.
    /// </summary>
    /// <param name="poolSize"></param>
    /// <param name="executorMemorySize"></param>
    /// <param name="driverMemorySize"></param>
    /// <param name="executorCores"></param>
    /// <param name="driverCores"></param>
    public SparkPoolConfigProperties(string poolSize, string executorMemorySize, string driverMemorySize, int executorCores, int driverCores)
    {
        this.PoolSize = poolSize;
        this.DriverMemorySize = driverMemorySize;
        this.DriverCores = driverCores;
        this.ExecutorMemorySize = executorMemorySize;
        this.ExecutorCores = executorCores;
    }
}
