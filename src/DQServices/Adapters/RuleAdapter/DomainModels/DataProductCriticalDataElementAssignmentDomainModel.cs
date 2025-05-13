namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;

using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using System.Collections.Generic;

internal class DataProductCriticalDataElementAssignmentDomainModel : DomainModel
{
    public override string FolderPath => DataEstateHealthConstants.SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataProductCriticalDataElementAssignment";

    private List<DatasetSchemaItemWrapper> _schema;
    public override List<DatasetSchemaItemWrapper> Schema => this._schema ??= SchemaUtils.GenerateSchemaFromDefinition([
        ["CriticalDataElementId", "String"],
        ["DataProductId", "String"]
    ]);
} 