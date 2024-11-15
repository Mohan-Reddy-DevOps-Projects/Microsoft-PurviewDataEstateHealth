namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.CatalogSparkJob.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DEHDQEvent
{
    public string AccountId { get; set; }
    public string JobId { get; set; }
    public DateTimeOffset JobTimestamp { get; set; }
}
