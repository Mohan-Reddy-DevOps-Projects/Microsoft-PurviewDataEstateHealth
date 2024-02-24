// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Security;

/// <summary>
/// The master key for the database.
/// </summary>
public class DatabaseMasterKey
{
    /// <summary>
    /// The database to use.
    /// </summary>
    public string DatabaseName { get; set; }

    /// <summary>
    /// The password
    /// </summary>
    public SecureString MasterKey { get; set; }
}
