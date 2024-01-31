namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions
{
    using System;

    public sealed class EntityValidationException : Exception
    {
        public EntityValidationException() { }

        public EntityValidationException(string message) : base(message)
        {
        }

        public EntityValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
