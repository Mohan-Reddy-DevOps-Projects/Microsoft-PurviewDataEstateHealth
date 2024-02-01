// -----------------------------------------------------------------------
// <copyright file="EntityRequiredValidatorAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityRequiredValidatorAttribute : EntityValidatorAttribute
    {
        public bool AllowNullOrEmpty { get; set; }

        public override void Validate(object value, string propName)
        {
            if (!this.AllowNullOrEmpty)
            {
                if (value == null || value is string strValue && String.IsNullOrEmpty(strValue))
                {
                    throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageNullOrEmpty, propName));
                }
            }
        }
    }
}
