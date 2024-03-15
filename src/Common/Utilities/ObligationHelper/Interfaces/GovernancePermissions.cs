// <copyright file="GovernancePermissions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces
{
    public static class GovernancePermissions
    {
        public static readonly string DgHealthRead = "Microsoft.Purview/datagovernance/datahealth/read";
        public static readonly string DgHealthWrite = "Microsoft.Purview/datagovernance/datahealth/write";

        public static readonly string BusinessDomainRead = "Microsoft.Purview/datagovernance/businessdomain/read";
        public static readonly string BusinessDomainWrite = "Microsoft.Purview/datagovernance/businessdomain/write";
        public static readonly string DataProductRead = "Microsoft.Purview/datagovernance/dataproduct/read";
        public static readonly string DataProductWrite = "Microsoft.Purview/datagovernance/dataproduct/write";
        public static readonly string OkrRead = "Microsoft.Purview/datagovernance/okr/read";
        public static readonly string OkrWrite = "Microsoft.Purview/datagovernance/okr/write";
        public static readonly string TermRead = "Microsoft.Purview/datagovernance/glossaryterm/read";
        public static readonly string TermWrite = "Microsoft.Purview/datagovernance/glossaryterm/write";

        public static readonly string ObserverRead = "Microsoft.Purview/datagovernance/dataquality/observer/read";
        public static readonly string ObserverWrite = "Microsoft.Purview/datagovernance/dataquality/observer/write";
    }
}
