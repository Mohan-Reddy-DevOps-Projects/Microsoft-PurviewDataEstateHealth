// -----------------------------------------------------------------------
// <copyright file="EntityNumberValidatorAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Collections;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityNumberValidatorAttribute : EntityValidatorAttribute
    {
        public double Max { get; set; }

        public double Min { get; set; }

        public EntityNumberValidatorAttribute()
        {
            this.Max = Double.MaxValue;
            this.Min = Double.MinValue;
        }

        public override void Validate(object value, string propName)
        {
            if (value is IEnumerable)
            {
                foreach (var item in value as IEnumerable)
                {
                    this.ValidateNumber(item, propName);
                }
            }
            else
            {
                this.ValidateNumber(value, propName);
            }
        }

        private void ValidateNumber(object value, string propName)
        {
            double numValue;
            try
            {
                if (value is int intValue)
                {
                    numValue = intValue;
                }
                numValue = Convert.ToDouble(value, null);
            }
            catch (Exception ex)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageTypeNotMatch, propName, "number"), ex);
            }

            if (numValue > this.Max)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageNumberExceedMax, propName, this.Max));
            }
            if (numValue < this.Min)
            {
                throw new EntityValidationException(FormatMessage(StringResources.ErrorMessageNumberBelowMin, propName, this.Min));
            }
        }
    }
}
