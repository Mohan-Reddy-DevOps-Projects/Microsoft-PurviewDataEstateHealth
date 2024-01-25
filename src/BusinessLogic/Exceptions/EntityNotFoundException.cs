// <copyright file="EntityNotFoundException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(ExceptionRefEntityInfo entity) : base(
            String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEntityDoesNotExist,
                String.IsNullOrEmpty(entity?.Type) ? "Entity" : entity.Type,
                String.Join(", ", entity?.Ids ?? new List<string>())))
        {
        }

        public EntityNotFoundException(ExceptionRefEntityInfo entity, Exception innerException) : base(
            String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEntityDoesNotExist,
                String.IsNullOrEmpty(entity?.Type) ? "Entity" : entity.Type,
                String.Join(", ", entity?.Ids ?? new List<string>())),
            innerException)
        {
        }

        public EntityNotFoundException() : base()
        {
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
