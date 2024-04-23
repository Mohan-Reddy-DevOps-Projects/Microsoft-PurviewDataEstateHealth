namespace Microsoft.Purview.DataEstateHealth.DHModels.Exceptions;

using System;

public class ParseMDQResultException : Exception
{
    public ParseMDQResultException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override string ToString()
    {
        return $"ParseMDQResultException, inner exception: {this.InnerException}";
    }
}
