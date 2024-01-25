// <copyright file="ExceptionRefEntityInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model
{
    using System.Collections.Generic;

    public class ExceptionRefEntityInfo
    {
        public List<string> Ids { get; set; }

        public string Type { get; set; }

        public ExceptionRefEntityInfo(string type, string id)
        {
            this.Ids = new List<string>() { id };
            this.Type = type;
        }

        public ExceptionRefEntityInfo(string type, List<string> ids)
        {
            this.Ids = ids;
            this.Type = type;
        }
    }
}
