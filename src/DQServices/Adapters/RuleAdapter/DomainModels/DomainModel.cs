namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using System.Collections.Generic;

internal abstract class DomainModel
{
    public abstract string FolderPath { get; }

    public abstract List<DatasetSchemaItemWrapper> Schema { get; }
}
