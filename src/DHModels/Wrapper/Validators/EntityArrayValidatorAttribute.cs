// -----------------------------------------------------------------------
// <copyright file="EntityArrayValidatorAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityArrayValidatorAttribute : EntityValidatorAttribute
    {
        public int MaxLength { get; set; }

        public int MinLength { get; set; }

        public EntityArrayValidatorAttribute()
        {
            this.MaxLength = int.MaxValue;
            this.MinLength = int.MinValue;
        }

        public override void Validate(object value, string propName)
        {
            IEnumerable<object> arrayValue;
            try
            {
                arrayValue = (IEnumerable<object>)value;
            }
            catch (Exception ex)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageTypeNotMatch, propName, typeof(IEnumerable).Name), ex);
            }

            if (arrayValue.Count() > this.MaxLength)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageArrayLengthExceedMax, propName, this.MaxLength));
            }
            if (arrayValue.Count() < this.MinLength)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageArrayLengthBelowMin, propName, this.MinLength));
            }
        }
    }
}
