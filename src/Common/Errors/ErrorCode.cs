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
    public static readonly ErrorCode StartupError = new(1010);

    #endregion

    #region Data access

    /// <summary>
    /// Problems accessing metadata service.
    /// </summary>
    public static readonly ErrorCode MetadataServiceException = new(3000);

    /// <summary>
    /// An exception while accessing the artifact store.
    /// </summary>
    public static readonly ErrorCode ArtifactStoreServiceException = new(3001);

    #endregion
}
