// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

public class UpsertMdqActionsPayload
{
    public Guid DQJobId { get; set; }

    public string JobStatus { get; set; }

    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public bool IsRetry { get; set; }

    public Guid RequestId { get; set; }

    public string ControlId { get; set; }
} 