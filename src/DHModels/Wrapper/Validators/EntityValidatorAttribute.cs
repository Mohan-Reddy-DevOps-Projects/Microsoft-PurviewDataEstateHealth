// -----------------------------------------------------------------------
// <copyright file="EntityValidatorAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators
{
    using System;
    using System.Globalization;

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class EntityValidatorAttribute : Attribute
    {
        public string ErrorMessageId { get; set; }

        protected string ErrorMessageString
        {
            get
            {
                if (!String.IsNullOrEmpty(this.ErrorMessageId))
                {
                    return FormatMessage(this.ErrorMessageId);
                }
                return null;
            }
        }

        public abstract void Validate(object value, string propName);

        protected static string FormatMessage(string template, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, template, args);
        }
    }
}
