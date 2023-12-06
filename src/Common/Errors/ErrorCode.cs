// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using Microsoft.DGP.ServiceBasics.Errors;

/// <summary>
/// The error code type for Data Estate Health exceptions.
/// </summary>
public class ErrorCode : ServiceErrorCode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCode"/> class.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    protected ErrorCode(int code, string message = null) : base(code, message)
    {
    }

    #region General

    /// <summary>
    /// An unknown error caused by a code bug.
    /// </summary>
    public static new readonly ErrorCode Unknown = new(ServiceErrorCode.Unknown.Code, ErrorMessage.Unknown);

    /// <summary>
    /// A required field is missing.
    /// </summary>
    public static new readonly ErrorCode MissingField = new(ServiceErrorCode.MissingField.Code, ServiceErrorCode.MissingField.Message);

    /// <summary>
    /// A field has an invalid value.
    /// </summary>
    public static new readonly ErrorCode InvalidField = new(ServiceErrorCode.InvalidField.Code, ServiceErrorCode.InvalidField.Message);

    /// <summary>
    /// Error in start up.
    /// </summary>
    public static readonly ErrorCode StartupError = new(1003);

    /// <summary>
    /// Missing client certificate.
    /// </summary>
    public static readonly ErrorCode MissingClientCertificate = new(1004);

    /// <summary>
    /// Invalid client certificate.
    /// </summary>
    public static readonly ErrorCode InvalidClientCertificate = new(1005);

    /// <summary>
    /// Not Authorized.
    /// </summary>
    public static readonly ErrorCode NotAuthorized = new(1006);

    /// <summary>
    /// Invalid certificate set
    /// </summary>
    public static readonly ErrorCode Invalid_CertificateSet = new(1007);

    /// <summary>
    /// Unsupported api-version query parameter.
    /// </summary>
    public static readonly ErrorCode UnsupportedApiVersionParameter = new(1008, "The API version '{0}' isn't supported. The supported versions are '{1}'.");

    /// <summary>
    /// The given location is not supported.
    /// </summary>
    public static readonly ErrorCode Unsupported_Location = new(1009);

    #endregion

    #region Storage checks

    /// <summary>
    /// Retrieving storage account access keys failed.
    /// </summary>
    public static readonly ErrorCode Storage_FailedToGetAccessKeys = new(2000);

    /// <summary>
    /// Could not find specified storage account table
    /// </summary>
    public static readonly ErrorCode Storage_TableDoesNotExist = new(2001);

    /// <summary>
    /// Storage exception
    /// </summary>
    public static readonly ErrorCode StorageException = new(2002);

    #endregion

    #region Job Errors

    /// <summary>
    /// Unhandled error in job processing
    /// </summary>
    public static readonly ErrorCode Job_UnhandledError = new(3000);

    /// <summary>
    /// Job encountered maximum number of retries
    /// </summary>
    public static readonly ErrorCode Job_MaximumRetryCount = new(3001);

    /// <summary>
    /// Job encountered maximum number of postponing
    /// </summary>
    public static readonly ErrorCode Job_MaximumPostponeCount = new(3002);

    /// <summary>
    /// Delete account dependencies job callback faulted
    /// </summary>
    public static readonly ErrorCode Job_DeleteAccountDependenciesJobCallbackFaulted = new(3003);

    /// <summary>
    /// Job has exceeded the defined execution parameters.
    /// </summary>
    public static readonly ErrorCode Job_ExecutionConstraintsExceeded = new(3004);

    #endregion

    #region Certificates

    /// <summary>
    /// Certificate not found
    /// </summary>
    public static readonly ErrorCode CertificateLoader_NotFound = new(40000);

    /// <summary>
    /// The certificate in akv does not have a private key
    /// </summary>
    public static readonly ErrorCode CertificateLoader_MissingPrivateKey = new(40001);

    #endregion

    #region KeyVault

    /// <summary>
    /// Error in reading certificate to key vault
    /// </summary>
    public static readonly ErrorCode KeyVault_GetCertificateError = new(50001);

    /// <summary>
    /// Error in reading secret to key vault
    /// </summary>
    public static readonly ErrorCode KeyVault_GetSecretError = new(50002);

    #endregion

    #region Data access

    /// <summary>
    /// Problems accessing metadata service.
    /// </summary>
    public static readonly ErrorCode MetadataServiceException = new(6000);

    /// <summary>
    /// An exception while accessing the artifact store.
    /// </summary>
    public static readonly ErrorCode ArtifactStoreServiceException = new(6001);

    #endregion

    #region Data Estate Health

    /// <summary>
    /// Invalid health report kind.
    /// </summary>
    public static readonly ErrorCode HealthReport_InvalidKind = new(7000);

    /// <summary>
    /// Health summary does not exist.
    /// </summary>
    public static readonly ErrorCode HealthSummary_NotAvailable = new(7001);

    /// <summary>
    /// Health actions not available.
    /// </summary>
    public static readonly ErrorCode HealthActions_NotAvailable = new(7002);

    /// <summary>
    /// Invalid health score kind.
    /// </summary>
    public static readonly ErrorCode HealthScore_InvalidKind = new(7003);

    /// <summary>
    /// Health scores not available.
    /// </summary>
    public static readonly ErrorCode HealthScores_NotAvailable = new(7004);

    /// <summary>
    /// Invalid health control kind.
    /// </summary>
    public static readonly ErrorCode HealthControl_InvalidKind = new(7005);

    /// <summary>
    /// Health trends not available.
    /// </summary>
    public static readonly ErrorCode HealthTrends_NotAvailable = new(7006);

    /// <summary>
    /// Invalid health trend column name.
    /// </summary>
    public static readonly ErrorCode HealthTrends_InvalidColumnName = new(7007);

    #endregion

    #region Profile

    /// <summary>
    /// Failed to create profile.
    /// </summary>
    public static readonly ErrorCode Profile_CreateFailed = new(8000);

    /// <summary>
    /// Profile not found.
    /// </summary>
    public static readonly ErrorCode Profile_NotFound = new(8001, "Profile not found.");

    #endregion

    #region Workspace

    /// <summary>
    /// Failed to get workspace.
    /// </summary>
    public static readonly ErrorCode Workspace_GetFailed = new(9000);

    /// <summary>
    /// Failed to create workspace.
    /// </summary>
    public static readonly ErrorCode Workspace_CreateFailed = new(9001);

    /// <summary>
    /// Workspace not found.
    /// </summary>
    public static readonly ErrorCode Workspace_NotFound = new(9002, "Workspace not found.");

    #endregion

    #region PowerBI

    /// <summary>
    /// PowerBI import has not reached a terminal state.
    /// </summary>
    public static readonly ErrorCode PowerBI_ImportNotCompleted = new(10000);

    /// <summary>
    /// PowerBI import failed.
    /// </summary>
    public static readonly ErrorCode PowerBI_ImportFailed = new(10001);

    /// <summary>
    /// PowerBI dataset conflict.
    /// </summary>
    public static readonly ErrorCode PowerBI_DatasetConflict = new(10002);

    /// <summary>
    /// PowerBI capacity not found.
    /// </summary>
    public static readonly ErrorCode PowerBI_CapacityNotFound = new(10003);

    /// <summary>
    /// PowerBI report delete failed.
    /// </summary>
    public static readonly ErrorCode PowerBI_ReportDeleteFailed = new(10004, "Report delete failed.");

    /// <summary>
    /// PowerBI dataset refresh failed.
    /// </summary>
    public static readonly ErrorCode PowerBI_DatasetRefreshFailed = new(10005, "Dataset refresh failed.");

    #endregion

    #region Partner

    /// <summary>
    /// Generic Partner Exception
    /// </summary>
    public static readonly ErrorCode PartnerException = new(11000);

    #endregion

    #region Async Operations

    /// <summary>
    /// Async operation not found
    /// </summary>
    public static readonly ErrorCode AsyncOperation_NotFound = new(12000);

    /// <summary>
    /// Async operation result not found
    /// </summary>
    public static readonly ErrorCode AsyncOperation_ResultNotFound = new(12001);

    /// <summary>
    /// Async operation source error not found
    /// </summary>
    public static readonly ErrorCode AsyncOperation_SourceErrorNotFound = new(12002);

    /// <summary>
    /// Async operation trigger failed
    /// </summary>
    public static readonly ErrorCode AsyncOperation_TriggerFailed = new(12003);

    /// <summary>
    /// Async operation execution failed
    /// </summary>
    public static readonly ErrorCode AsyncOperation_ExecutionFailed = new(12004);

    /// <summary>
    /// Async operation timeout
    /// </summary>
    public static readonly ErrorCode AsyncOperation_Timeout = new(12005);

    /// <summary>
    /// Async operation precondition failed
    /// </summary>
    public static readonly ErrorCode AsyncOperation_PreconditionFailed = new(12006);

    #endregion
}
