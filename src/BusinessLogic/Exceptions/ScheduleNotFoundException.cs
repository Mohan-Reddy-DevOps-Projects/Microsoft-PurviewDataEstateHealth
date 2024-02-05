namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using System;

    public sealed class ScheduleNotFoundException : Exception
    {
        public ScheduleNotFoundException() { }

        public ScheduleNotFoundException(string message) : base(message)
        {
        }

        public ScheduleNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
