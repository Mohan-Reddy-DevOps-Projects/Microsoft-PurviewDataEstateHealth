// <copyright file="EntityReferencedException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using System;

    public class EntityReferencedException : Exception
    {
        public EntityReferencedException(string message) : base(
            message)
        {
        }

        public EntityReferencedException(string message, Exception innerException) : base(
           message, innerException)
        {
        }

        public EntityReferencedException() : base()
        {
        }
    }
}
