// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// https://github.com/microsoft/powerbi-powershell/blob/master/src/Common/Common.Api/Datasets/DatasetStorage.cs
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
internal enum StorageMode
{
    /// <summary>
    /// 
    /// </summary>
    Unknown,

    /// <summary>
    /// 
    /// </summary>
    Abf,

    /// <summary>
    /// 
    /// </summary>
    PremiumFiles,
}
