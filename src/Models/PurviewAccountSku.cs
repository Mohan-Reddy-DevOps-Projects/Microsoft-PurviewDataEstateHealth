// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Purview Account Sku
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum PurviewAccountSku
{
    /// <summary>
    /// Classic Azure account.
    /// </summary>
    Classic = 1,

    /// <summary>
    /// Enterprise tier tenant account.
    /// </summary>
    Enterprise,

    /// <summary>
    /// Free tier tenant account.
    /// </summary>
    Free
}
