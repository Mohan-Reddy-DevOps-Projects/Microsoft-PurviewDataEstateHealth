// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;

internal static class SystemDatasets
{
    private static readonly IReadOnlyDictionary<string, IDataset> allowedDatasets = new Dictionary<string, IDataset>()
    {
        { HealthDataset.Dataset.DataGovernance.ToString(), new HealthDataset(HealthDataset.Dataset.DataGovernance, "Data_Governance_Dataset") }
    };

    public static IReadOnlyDictionary<string, IDataset> Get() => allowedDatasets;
}

internal sealed class HealthDataset : IDataset
{
    public enum Dataset
    {
        DataGovernance
    }

    public HealthDataset(Dataset dataset, string name)
    {
        this.Name = name;
        this.Value = dataset.ToString();
    }

    public string Name { get; }

    public string Value { get; }
}
