namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;

using Microsoft.DGP.ServiceBasics.Errors;

[Serializable]
public class ExtendedServiceException : ServiceException
{
    public string ErrorCode { get; set; } = null;

    public ExtendedServiceException(ServiceError serviceError, string errorCode, Exception innerException = null)
        : base(serviceError.Message, innerException)
    {
        this.ErrorCode = errorCode;
    }

    public ExtendedServiceException(ErrorCategory category, string errorCode, string errorMessage)
    : base(new ServiceError(category, 0, errorMessage))
    {
        this.ErrorCode = errorCode;
    }
}

