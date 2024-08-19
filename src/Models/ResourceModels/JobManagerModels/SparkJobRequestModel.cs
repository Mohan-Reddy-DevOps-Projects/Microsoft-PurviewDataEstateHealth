namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.JobManagerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SparkJobRequestModel
{
    public Uri sasUri { get; set; }
    public string accountId { get; set; }
    public string containerName { get; set; }
    public string sinkLocation { get; set; }
    public string jobId { get; set; }
    public string jarClassName { get; set; }
    public string miToken { get; set; }
    public string storageUrl { get; set; }

    public string storageType { get; set; }
    public string cosmosDBEndpoint { get; set; } = "";
    public string cosmosDBKey { get; set; } = "";
    public string workSpaceID { get; set; } = "";
}
