// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// The known contexts a worker job executes within.
///
/// This enum uses flags, which allows us to derive additional context for each job.  New base type enum values must
/// be powers of two, which is easily achieved by using the pattern that increments the number on the right hand side
/// of the expression.
///
/// </summary>
[Flags]
[JsonConverter(typeof(StringEnumConverter))]
public enum WorkerJobExecutionContext
{
    #region base

    /// <summary>
    /// The job is running within no special context.
    /// </summary>
    None = 0, // 0

    /// <summary>
    /// The child job execution context.
    /// </summary>
    ChildJob = 1 << 0, // 1

    /// <summary>
    /// The account deletion context.
    /// </summary>
    DeleteAccountJob = 1 << 1, // 2

    /// <summary>
    /// The provider job context.
    /// </summary>
    ProviderJob = 1 << 2, // 4

    /// <summary>
    /// The consumer job context.
    /// </summary>
    ConsumerJob = 1 << 3, // 8

    #endregion

    #region combinations

    /// <summary>
    /// The Delete Account child job context.
    /// </summary>
    DeleteAccountChildJob = ChildJob | DeleteAccountJob,

    /// <summary>
    /// The provider child job context.
    /// </summary>
    ProviderChildJob = ChildJob | ProviderJob,

    /// <summary>
    /// The consumer child job context.
    /// </summary>
    ConsumerChildJob = ChildJob | ConsumerJob

    #endregion
}
