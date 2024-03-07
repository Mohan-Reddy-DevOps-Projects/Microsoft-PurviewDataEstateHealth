namespace Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class JoinAdapterResult
{
    public string JoinSql { get; set; }

    public List<InputDatasetWrapper> inputDatasetsFromJoin { get; set; }

    public List<SparkSchemaItemWrapper> SchemaFromJoin { get; set; }
}
