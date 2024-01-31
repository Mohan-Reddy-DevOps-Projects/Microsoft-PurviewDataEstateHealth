// -----------------------------------------------------------------------
// <copyright file="EntityIdValidatorAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.ActiveGlossary.Models.Validators
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityIdValidatorAttribute : EntityValidatorAttribute
    {
        private const string defaultIdPattern = @"^[A-Za-z0-9_-]+$";
        private const int defaultMaxLength = 36;
        private const int defaultMinLength = 1;
        private readonly EntityStringValidatorAttribute stringValidator = new EntityStringValidatorAttribute();
        private readonly EntityRegexValidatorAttribute regexValidator = new EntityRegexValidatorAttribute();

        public EntityIdValidatorAttribute(string pattern = defaultIdPattern, int maxLength = defaultMaxLength, int minLength = defaultMinLength)
        {
            this.regexValidator.Pattern = pattern;
            this.regexValidator.ErrorMessageId = StringResources.ErrorMessageInvalidEntityId;

            this.stringValidator.MaxLength = maxLength;
            this.stringValidator.MinLength = minLength;
        }

        public override void Validate(object value, string propName)
        {
            try
            {
                this.stringValidator.Validate(value, propName);
                this.regexValidator.Validate(value, propName);
            }
            catch (EntityValidationException ex)
            {
                if (String.IsNullOrEmpty(this.ErrorMessageString))
                {
                    throw;
                }
                else
                {
                    throw new EntityValidationException(this.ErrorMessageString, ex);
                }
            }
        }
    }
}
