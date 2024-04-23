namespace Microsoft.Purview.DataEstateHealth.DHModels.Exceptions;

using System;

public class PurgeObserverException : Exception
{
    public PurgeObserverException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override string ToString()
    {
        return $"PurgeObserverException, inner exception: {this.InnerException}";
    }
}
