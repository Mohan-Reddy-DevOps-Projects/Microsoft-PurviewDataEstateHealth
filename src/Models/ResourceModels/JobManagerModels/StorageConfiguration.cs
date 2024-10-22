namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.JobManagerModels;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StorageConfiguration
{
    public string Type { get; set; }
    public TypeProperties TypeProperties { get; set; }
    public string Id { get; set; }
    public SystemData SystemData { get; set; }
    public string Status { get; set; }
}

public class TypeProperties
{
    public string LocationURL { get; set; }
    public string Endpoint { get; set; }
}

public class SystemData
{
    public string CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastModifiedAt { get; set; }
    public string LastModifiedBy { get; set; }
}


