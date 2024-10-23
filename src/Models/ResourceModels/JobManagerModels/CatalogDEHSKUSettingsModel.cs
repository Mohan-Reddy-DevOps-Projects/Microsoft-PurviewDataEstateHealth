namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.JobManagerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Feature
{
    public string Mode { get; set; }
}

public class Features
{
    public Feature DataEstateHealth { get; set; }
    public Feature DataQuality { get; set; }
}

public class CatalogDEHSKUSettingsModel
{
    public Guid Id { get; set; }
    public string Sku { get; set; }
    public Features Features { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}
