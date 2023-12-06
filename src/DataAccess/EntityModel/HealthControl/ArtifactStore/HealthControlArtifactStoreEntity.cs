// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Purview.DataAccess.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

/// <inheriteddoc/>
public class HealthControlArtifactStoreEntity : ArtifactStoreEntity, IHealthControlArtifactStoreEntity
{
    /// <inheriteddoc/>
    public override DataEstateHealthEntityTypes GetEntityType() => DataEstateHealthEntityTypes.DataEstateHealthControl;

    /// <inheriteddoc/>
    [JsonProperty("kind")]
    public HealthControlKind HealthControlKind { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("parentControlId")]
    public Guid ParentControlId { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("isCompositeControl")]
    public bool IsCompositeControl { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("controlType")]
    public HealthResourceType ControlType { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("ownerContact")]
    public OwnerContact OwnerContact { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("targetScore")]
    public int TargetScore { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("scoreUnit")]
    public string ScoreUnit { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("healthStatus")]
    public string HealthStatus { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("controlStatus")]
    public HealthResourceStatus ControlStatus { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("startsAt")]
    public DateTime StartsAt { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("endsAt")]
    public DateTime EndsAt { get; set; }

    /// <inheriteddoc/>
    [JsonProperty("trendUrl")]
    public string TrendUrl { get; set; }

    /// <inheriteddoc/>
    public override Dictionary<string, string> GetIndexedProperties()
    {
        return new Dictionary<string, string>()
        {
            {
                nameof(ObjectId).UncapitalizeFirstChar(),
                this.ObjectId.ToString()
            },
            {
                nameof(IsCompositeControl).UncapitalizeFirstChar(),
                this.IsCompositeControl.ToString()
            },
            {
                nameof(ParentControlId).UncapitalizeFirstChar(),
                this.ParentControlId.ToString()
            },
            {
                nameof(ControlType).UncapitalizeFirstChar(),
                this.ControlType.ToString()
            },
            {
                nameof(Name).UncapitalizeFirstChar(),
                this.Name.ToString()
            },
            {
                $"{nameof(OwnerContact).UncapitalizeFirstChar()}{nameof(HealthControlArtifactStoreEntity.OwnerContact.ObjectId).UncapitalizeFirstChar()}",
                this.OwnerContact.ObjectId.ToString()
            },
            {
                nameof(ControlStatus).UncapitalizeFirstChar(),
                this.ControlStatus.ToString()
            },
            {
                $"{nameof(OwnerContact).UncapitalizeFirstChar()}{nameof(OwnerContact.DisplayName).UncapitalizeFirstChar()}",
                this.OwnerContact.DisplayName.ToString()
            },
            {
                nameof(HealthControlKind).UncapitalizeFirstChar(),
                this.HealthControlKind.ToString()
            }
        };
    }

    /// <inheriteddoc/>
    public override void Validate(ICommonFieldValidationService fieldValidationService) => base.Validate(fieldValidationService);
}
