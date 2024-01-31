// -----------------------------------------------------------------------
// <copyright file="EntityNameValidatorAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.ActiveGlossary.Models.Validators
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityNameValidatorAttribute : EntityValidatorAttribute
    {
        private const int defaultMaxLength = 256;
        private const int defaultMinLength = 1;
        private readonly EntityStringValidatorAttribute stringValidator = new EntityStringValidatorAttribute();
        private readonly EntityRegexValidatorAttribute printableNameValidator = new EntityRegexValidatorAttribute(@"^(\P{Cc})+$");
        private readonly EntityRegexValidatorAttribute notEmptyNameValidator = new EntityRegexValidatorAttribute(@"^.*\S.*$");
        private readonly EntityRegexValidatorAttribute regexValidator = new EntityRegexValidatorAttribute();

        public EntityNameValidatorAttribute(string pattern = null, int maxLength = defaultMaxLength, int minLength = defaultMinLength)
        {
            this.regexValidator.Pattern = pattern;

            this.printableNameValidator.ErrorMessageId = StringResources.ErrorMessageInvalidEntityName;
            this.notEmptyNameValidator.ErrorMessageId = StringResources.ErrorMessageEmptyEntityName;

            this.stringValidator.MaxLength = maxLength;
            this.stringValidator.MinLength = minLength;
        }

        public override void Validate(object value, string propName)
        {
            try
            {
                this.stringValidator.Validate(value, propName);
                if (this.regexValidator.Pattern != null)
                {
                    this.regexValidator.Validate(value, propName);
                }
                else
                {
                    this.notEmptyNameValidator.Validate(value, propName);
                    this.printableNameValidator.Validate(value, propName);
                }
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
