namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using System;

    public sealed class ControlNotMatchedException : Exception
    {
        public ControlNotMatchedException() { }

        public ControlNotMatchedException(string message) : base(message)
        {
        }

        public ControlNotMatchedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
