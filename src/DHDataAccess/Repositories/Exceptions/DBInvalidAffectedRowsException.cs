namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Exceptions;
using System;

public sealed class DBInvalidAffectedRowsException : Exception
{
    public DBInvalidAffectedRowsException() { }

    public DBInvalidAffectedRowsException(string message) : base(message)
    {
    }

    public DBInvalidAffectedRowsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}