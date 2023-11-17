// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Core;

/// <summary>
/// 
/// </summary>
public interface IDeltaLakeOperatorFactory
{
    /// <summary>
    /// Build DeltaLakeOperator instance
    /// </summary>
    /// <param name="adlsGen2FileSystemEndpoint"></param>
    /// <param name="tokenCredential"></param>
    /// <returns></returns>
    public IDeltaLakeOperator Build(Uri adlsGen2FileSystemEndpoint, TokenCredential tokenCredential);
}
