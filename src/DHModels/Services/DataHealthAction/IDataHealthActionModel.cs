// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.Models;

using System;

public interface IDataHealthActionModel
{
    Guid Id { get; set; }

    DataHealthActionCategory Category { get; set; }

    string FindingType { get; set; }

    string FindingSubType { get; set; }

    string FindingName { get; set; }

    string ActionDetail { get; set; }

    Guid DomainId { get; set; }

    string Deadline { get; set; }

    DataHealthActionStatus Status { get; set; }

    DataHealthActionSeverityLevel SeverityLevel { get; set; }

    TargetEntityType TargetEntityType { get; set; }

    string TargetId { get; set; }

    string[] Owners { get; set; }

    DataHealthActionSystemData SystemData { get; set; }

}


