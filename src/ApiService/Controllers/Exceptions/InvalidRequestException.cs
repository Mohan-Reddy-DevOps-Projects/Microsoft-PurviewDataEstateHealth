namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Exceptions;

public class InvalidRequestException : Exception
{
    public InvalidRequestException(string message) : base(message)
    {
    }

    public InvalidRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InvalidRequestException()
    {
    }
}
