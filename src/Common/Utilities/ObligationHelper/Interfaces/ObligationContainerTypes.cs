// <copyright file="ObligationContainerTypes.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces
{
    public static class ObligationContainerTypes
    {
        public static readonly string BusinessDomain = $"{Obligation.ContainerTypePrefix}{RepositoryEntityTypes.Domain}";
        public static readonly string DataGovernanceApp = $"{Obligation.ContainerTypePrefix}{RepositoryEntityTypes.DataGovernanceApp}";
        public static readonly string DataGovernanceScope = $"{Obligation.ContainerTypePrefix}{RepositoryEntityTypes.DataGovernanceScope}";
    }

    public static class RepositoryEntityTypes
    {
        public static readonly string Domain = "BusinessDomain";
        public static readonly string DataGovernanceApp = "DataGovernanceApp";
        public static readonly string DataGovernanceScope = "DGDataQualityScope";
    }

    public static class ObligationType
    {
        public const string NotApplicable = "NotApplicable";
        public const string Permit = "Permit";
    }
}
