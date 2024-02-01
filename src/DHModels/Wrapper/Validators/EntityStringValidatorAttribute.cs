// -----------------------------------------------------------------------
// <copyright file="EntityStringValidatorAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityStringValidatorAttribute : EntityValidatorAttribute
    {
        public int MaxLength { get; set; }

        public int MinLength { get; set; }

        public EntityStringValidatorAttribute()
        {
            this.MaxLength = int.MaxValue;
            this.MinLength = int.MinValue;
        }

        public override void Validate(object value, string propName)
        {
            string str;
            try
            {
                str = (string)value;
            }
            catch (Exception ex)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageTypeNotMatch, propName, typeof(string).Name), ex);
            }

            if (str.Length > this.MaxLength)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageStringLengthMax, propName, this.MaxLength, str.Length));
            }
            if (str.Length < this.MinLength)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageStringLengthMin, propName, this.MinLength, str.Length));
            }
        }
    }
}
