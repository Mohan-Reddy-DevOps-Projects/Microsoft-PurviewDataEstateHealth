﻿namespace Microsoft.Purview.DataEstateHealth.DHModels.Models;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using System.Collections.Generic;

public class RuleAdapterResult
{
    public string ProjectionSql { get; set; }

    public List<InputDatasetWrapper> inputDatasetsFromJoin { get; set; }

    public List<SparkSchemaItemWrapper> SchemaFromJoin { get; set; }

    public List<CustomTruthRuleWrapper> CustomRules { get; set; }
}
