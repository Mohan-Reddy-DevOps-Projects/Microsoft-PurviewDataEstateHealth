namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using System;

    public sealed class ControlNotFoundException : Exception
    {
        public ControlNotFoundException() { }

        public ControlNotFoundException(string message) : base(message)
        {
        }

        public ControlNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
