// <copyright file="UnsupportedParamException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions
{
    using System;

    public class UnsupportedParamException : Exception
    {
        public UnsupportedParamException(string message) : base(message)
        {
        }

        public UnsupportedParamException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnsupportedParamException()
        {
        }
    }
}
