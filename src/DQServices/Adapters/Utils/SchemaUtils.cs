namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.Utils;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetSchemaItem;

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

public static class SchemaUtils
{
    public static List<DatasetSchemaItemWrapper> GenerateSchemaFromDefinition(string[][] colsDef)
    {
        return colsDef.Select(colDef =>
        {
            var col = DatasetSchemaItemWrapper.Create(new JObject()
            {
                { DynamicEntityWrapper.keyType, colDef[1] },
            });

            col.Name = colDef[0];

            if (col is DatasetSchemaNumberItemWrapper numberCol)
            {
                numberCol.Integral = colDef[2] == "true";
            }
            return col;
        }).ToList();
    }

    public static List<SparkSchemaItemWrapper> GenerateSparkSchemaFromDefinition(string[][] colsDef)
    {
        return colsDef.Select(colDef =>
        {
            var col = new SparkSchemaItemWrapper(new JObject());
            col.Name = colDef[0];
            col.ColumnPath = colDef[0];
            col.Type = colDef[1];
            return col;
        }).ToList();
    }
}
