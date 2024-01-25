// <copyright file="ExceptionRefEntityField.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model
{
    public class ExceptionRefEntityField
    {
        public string Type { get; set; }

        public string FieldName { get; set; }

        public string FieldValue { get; set; }

        public ExceptionRefEntityField(string type, string fieldName, string fieldValue)
        {
            this.Type = type;
            this.FieldName = fieldName;
            this.FieldValue = fieldValue;
        }
    }
}
