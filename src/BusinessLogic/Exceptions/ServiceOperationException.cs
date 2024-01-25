// <copyright file="ServiceOperationException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using System;

    public class ServiceOperationException : Exception
    {
        public ServiceOperationException(string message) : base(message)
        {
        }

        public ServiceOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ServiceOperationException()
        {
        }
    }
}
