// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using System;
using System.Threading.Tasks;

/// <summary>
/// Job Dispatcher contract.
/// </summary>
public interface IJobDispatcher : IDisposable
{
    /// <summary>
    /// Initializes the Job Dispatcher
    /// </summary>
    public Task Initialize();
}
