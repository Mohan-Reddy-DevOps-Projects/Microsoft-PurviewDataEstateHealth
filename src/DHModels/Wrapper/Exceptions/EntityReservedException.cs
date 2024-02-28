namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using System;

internal class EntityReservedException : Exception
{
    public EntityReservedException() { }

    public EntityReservedException(string message) : base(message)
    {
    }

    public EntityReservedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
