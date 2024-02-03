// -----------------------------------------------------------------------
// <copyright file="EntityRegexValidatorAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityRegexValidatorAttribute : EntityValidatorAttribute
    {
        private Regex regex;

        private string pattern;

        public string Pattern
        {
            get => this.pattern;
            set
            {
                this.pattern = value;
                this.regex = !String.IsNullOrEmpty(this.Pattern) ? new Regex(this.Pattern) : null;
            }
        }

        public EntityRegexValidatorAttribute()
        {
        }

        public EntityRegexValidatorAttribute(string pPattern)
        {
            this.Pattern = pPattern;
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
            if (this.regex != null && !this.regex.IsMatch(str))
            {
                throw new EntityValidationException(!String.IsNullOrEmpty(this.ErrorMessageString) ? this.ErrorMessageString : FormatMessage(StringResources.ErrorMessageRegularExpressionNotMatch, propName, this.Pattern));
            }
        }
    }
}
