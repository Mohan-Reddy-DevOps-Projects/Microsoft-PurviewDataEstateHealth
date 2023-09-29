// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using System.Threading.Tasks;

/// <summary>
/// Interface for PowerBI service
/// </summary>
public interface IPowerBIService
{
    /// <summary>
    /// Initialize the PowerBI client
    /// </summary>
    /// <returns></returns>
    Task Initialize();
}
