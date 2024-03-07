namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;

public static class ErrorResponseCode
{
    public static readonly string DataHealthNotAuthorized = "DataHealthNotAuthorized";
    public static readonly string DataHealthInvalidEntity = "DataHealthInvalidEntity";
    public static readonly string DataHealthInternalError = "DataHealthInternalError";
    public static readonly string DataHealthNotFoundError = "DataHealthNotFoundError";
    public static readonly string DataHealthConflictError = "DataHealthConflictError";
    public static readonly string DataHealthEntityReferencedError = "DataHealthEntityReferencedError";
    public static readonly string DataHealthForbiddenError = "DataHealthForbiddenError";
    public static readonly string DataHealthTooManyRequestsError = "DataHealthTooManyRequestsError";
}
