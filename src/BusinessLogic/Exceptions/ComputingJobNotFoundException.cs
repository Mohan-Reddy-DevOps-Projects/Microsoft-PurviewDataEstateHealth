namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using System;

    public sealed class ComputingJobNotFoundException : Exception
    {
        public ComputingJobNotFoundException() { }

        public ComputingJobNotFoundException(string message) : base(message)
        {
        }

        public ComputingJobNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
