// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Data.DeltaLake.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// IDeltaLakeOperator
/// </summary>
public interface IDeltaLakeOperator
{
    /// <summary>
    /// Create or overwrite dataset.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableRelativePath"></param>
    /// <param name="schema"></param>
    /// <param name="rows"></param>
    /// <returns></returns>
    Task CreateOrOverwriteDataset<T>(string tableRelativePath, StructType schema, IList<T> rows);

    /// <summary>
    /// Create Or append dataset.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableRelativePath"></param>
    /// <param name="schema"></param>
    /// <param name="rows"></param>
    /// <returns></returns>
    Task CreateOrAppendDataset<T>(string tableRelativePath, StructType schema, IList<T> rows);

    /// <summary>
    /// Read dataset
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableRelativePath"></param>
    /// <returns></returns>
    Task<IList<T>> ReadDataset<T>(string tableRelativePath) where T : new();
}
