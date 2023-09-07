// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;

internal class DataEstateHealthFabricProvisioningJobMetadata : StagedWorkerJobMetadata
{
    /// <summary>
    /// Purview account ID
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Purview account tenant id
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Purview account catalog ID
    /// </summary>
    public Guid CatalogId { get; set; }

    /// <summary>
    /// Fabric Workspace Id
    /// </summary>
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// Fabric Lakehouse Id
    /// </summary>
    public Guid LakehouseId { get; set; }

    /// <summary>
    /// Fabric pipeline Id
    /// </summary>
    public Guid PipelineId { get; set; }

    /// <summary>
    /// Fabric event stream Id
    /// </summary>
    public Guid EventStreamId { get; set; }

    /// <summary>
    /// Fabric data warehouse Id
    /// </summary>
    public Guid DataWarehouseId { get; set; }

    /// <summary>
    /// The flag for workspace provisioning
    /// </summary>
    public bool IsWorkspaceProvisioned { get; set; }

    /// <summary>
    /// The flag for lakehouse provisioning
    /// </summary>
    public bool IsLakehouseProvisioned { get; set; }

    /// <summary>
    /// The flag for provisioning pipeline for OT Data
    /// </summary>
    public bool IsPipelineProvisioned { get; set; }

    /// <summary>
    /// The flag for provisioning event stream for business metadata
    /// </summary>
    public bool IsEventStreamProvisioned { get; set; }

    /// <summary>
    /// The flag for data warehouse provisioning
    /// </summary>
    public bool IsDataWarehouseProvisioned { get; set; }

    /// <summary>
    /// The flag for persisting metadata
    /// </summary>
    public bool IsMetadataPersisted { get; set; }

}
