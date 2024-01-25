// <copyright file="EntityForbiddenException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using System;

    public class EntityForbiddenException : Exception
    {
        public EntityForbiddenException(string message) : base(
    message)
        {
        }

        public EntityForbiddenException(string message, Exception innerException) : base(
           message, innerException)
        {
        }

        public EntityForbiddenException() : base()
        {
        }
    }
}
