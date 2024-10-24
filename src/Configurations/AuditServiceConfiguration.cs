// -----------------------------------------------------------------------
// <copyright file="AuditServiceConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Purview.DataGovernance.AuditAPI.Entities;

public class AuditServiceConfiguration
{
    public const string ConfigSectionName = "AuditService";

    public AuditServiceAADAuthConfiguration AADAuth { get; set; }

    public CDPEnvironment AuditEnvironment { get; set; }
}

public class AuditServiceAADAuthConfiguration
{
    public string Authority { get; set; }

    public string ClientId { get; set; }

    public string CertificateName { get; set; }
}

