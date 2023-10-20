// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;

[HealthControlEntity(HealthControlKind.DataGovernance)]
internal class DataGovernanceHealthControlEntity : HealthControlEntity
{
    public DataGovernanceHealthControlEntity()
    {
    }

    public DataGovernanceHealthControlEntity(DataGovernanceHealthControlEntity entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsCompositeControl = entity.IsCompositeControl;
        this.ControlType = entity.ControlType;
        this.OwnerContact = entity.OwnerContact;
        this.CurrentScore = entity.CurrentScore;
        this.TargetScore = entity.TargetScore;
        this.ScoreUnit = entity.ScoreUnit;
        this.HealthStatus = entity.HealthStatus;
        this.ControlStatus = entity.ControlStatus;
        this.CreatedAt = entity.CreatedAt;
        this.StartsAt = entity.StartsAt;
        this.EndsAt = entity.EndsAt;
        this.TrendUrl = entity.TrendUrl;
        this.BusinessDomainsListLink = entity.BusinessDomainsListLink;
    }
}
