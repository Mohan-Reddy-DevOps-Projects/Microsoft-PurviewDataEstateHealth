// <copyright file="EntityConflictException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class EntityConflictException : Exception
    {
        public EntityConflictException(ExceptionRefEntityInfo entity) : base(
            String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEntityAlreadyExists,
                String.IsNullOrEmpty(entity?.Type) ? "Entity" : entity.Type,
                String.Join(", ", entity?.Ids ?? new List<string>())))
        {
        }

        public EntityConflictException(ExceptionRefEntityField field) : base(
            String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEntityFieldAlreadyExists,
                String.IsNullOrEmpty(field?.Type) ? "Entity" : field.Type,
                field.FieldName,
                field.FieldValue))
        {
        }

        public EntityConflictException(ExceptionRefEntityInfo entity, Exception innerException) : base(
            String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEntityAlreadyExists,
                String.IsNullOrEmpty(entity?.Type) ? "Entity" : entity.Type,
                String.Join(", ", entity?.Ids ?? new List<string>())),
            innerException)
        {
        }

        public EntityConflictException(ExceptionRefEntityField field, Exception innerException) : base(
            String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEntityFieldAlreadyExists,
                String.IsNullOrEmpty(field?.Type) ? "Entity" : field.Type,
                field.FieldName,
                field.FieldValue),
            innerException)
        {
        }

        public EntityConflictException() : base()
        {
        }

        public EntityConflictException(string message) : base(message)
        {
        }

        public EntityConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
