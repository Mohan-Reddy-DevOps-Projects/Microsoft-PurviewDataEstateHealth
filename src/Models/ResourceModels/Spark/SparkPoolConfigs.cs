// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;

/// <summary>
/// Spark pool config properties.
/// </summary>
public sealed class SparkPoolConfigs
{
    private const string Small = "small";
    private const string Medium = "medium";
    private const string Large = "large";
    private static readonly Dictionary<string, SparkPoolConfigProperties> options = new(StringComparer.OrdinalIgnoreCase)
    {
        { Small, new SparkPoolConfigProperties(Small,"28g","28g",4,4) },
        { Medium, new SparkPoolConfigProperties(Medium,"56g","56g",8,8) },
        { Large, new SparkPoolConfigProperties(Large,"112g","112g",16,16) }
    };

    /// <summary>
    /// Gets the spark pool configs dictionary.
    /// </summary>
    public static IDictionary<string, SparkPoolConfigProperties> Options = options;
}
