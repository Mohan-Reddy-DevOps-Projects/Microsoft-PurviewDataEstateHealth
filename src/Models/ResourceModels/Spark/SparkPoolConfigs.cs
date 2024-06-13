// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;

/// <summary>
/// Spark pool config properties.
/// </summary>
public sealed class SparkPoolConfigs
{
    // @see: https://learn.microsoft.com/en-us/azure/synapse-analytics/spark/apache-spark-pool-configurations#node-sizes
    /*
    Size	                        vCore	Memory
    Small	                        4	    32 GB
    Medium	                        8	    64 GB
    Large	                        16	    128 GB
    XLarge	                        32	    256 GB
    XXLarge	                        64	    432 GB
    XXX Large (Isolated Compute)	80	    504 GB
    */
    private const string Small = "small";
    private const string Medium = "medium";
    private const string Large = "large";
    private const string XLarge = "xlarge";
    private const string XXLarge = "xxlarge";
    private const string XXXLarge = "xxxlarge";

    private static readonly Dictionary<string, SparkPoolConfigProperties> options = new(StringComparer.OrdinalIgnoreCase)
    {
        { Small, new SparkPoolConfigProperties(Small,"28g","28g",4,4) },
        { Medium, new SparkPoolConfigProperties(Medium,"56g","56g",8,8) },
        { Large, new SparkPoolConfigProperties(Large,"112g","112g",16,16) },
        { XLarge, new SparkPoolConfigProperties(XLarge,"224g","224g",32,32) },
        { XXLarge, new SparkPoolConfigProperties(XXLarge,"378g","378g",64,64) },
        { XXXLarge, new SparkPoolConfigProperties(XXXLarge,"441g","441g",80,80) }
    };

    /// <summary>
    /// Gets the spark pool configs dictionary.
    /// </summary>
    public static IDictionary<string, SparkPoolConfigProperties> Options = options;
}
