namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class CBSReceiptStatus
{
    /// <summary>
    /// CBS accepted the submission 
    /// </summary>
    public const string Succeded = "Succeeded";
    /// <summary>
    /// We do not got feedback yet
    /// </summary>
    public const string Unknown = "Unknown";
    /// <summary>
    /// Validation errors were provided by CBS
    /// </summary>
    public const string Failed = "Failed";
    /// <summary>
    /// Validation Errors happened but we are not able to correlate CBS details with our current batch
    /// </summary>
    public const string BatchFailure = "BatchFailure";
    /// <summary>
    /// Result code from CBS is not Ok or PartialContent
    /// </summary>
    public const string UnknownUnsuccesfulHttpCode = "UnknownUnsuccesfulHttpCode";
}
