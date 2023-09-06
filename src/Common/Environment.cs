// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Defines the various environments for Purview Share
/// </summary>
public enum Environment
{
    /// <summary>
    /// Localhost environment
    /// </summary>
    Development,

    /// <summary>
    /// Dogfood environment
    /// </summary>
    Dogfood,

    /// <summary>
    /// The dev environment
    /// </summary>
    Dev,

    /// <summary>
    /// CI environment
    /// </summary>
    Ci,

    /// <summary>
    /// Int environment
    /// </summary>
    Int,

    /// <summary>
    /// Perf environment
    /// </summary>
    Perf,

    /// <summary>
    /// Canary environment
    /// </summary>
    Canary,

    /// <summary>
    /// Production environment
    /// </summary>
    Production
}
