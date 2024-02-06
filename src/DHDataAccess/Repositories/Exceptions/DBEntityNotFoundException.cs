namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Exceptions;
using System;

public sealed class DBEntityNotFoundException : Exception
{
    public DBEntityNotFoundException() { }

    public DBEntityNotFoundException(string message) : base(message)
    {
    }

    public DBEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}