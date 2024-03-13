namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class DHRawScore(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyEntityPayload = "entityPayload";
    private const string keyScores = "scores";
    private const string keyEntityType = "entityType";

    public DHRawScore() : this([]) { }

    [EntityProperty(keyEntityType)]
    [EntityRequiredValidator]
    public RowScoreEntityType EntityType
    {
        get
        {
            var enumStr = this.GetPropertyValue<string>(keyEntityType);
            return Enum.TryParse<RowScoreEntityType>(enumStr, true, out var result) ? result : throw new EntityValidationException($"Failed to parse entityType: {enumStr}");
        }
        set => this.SetPropertyValue(keyEntityType, value.ToString());
    }

    /**
     * For DataProduct
     * {
     *   DataProductID - string
     *   DataProductDisplayName - string
     *   BusinessDomainId - string
     *   DataProductStatusDisplayName - string
     *   DataProductOwnerIds - string
     * }
     **/
    [EntityProperty(keyEntityPayload)]
    public JObject EntityPayload
    {
        get => this.GetPropertyValue<JObject>(keyEntityPayload);
        set => this.SetPropertyValue(keyEntityPayload, value);
    }

    private IEnumerable<DHScoreUnitWrapper>? scores;

    [EntityProperty(keyScores)]
    public IEnumerable<DHScoreUnitWrapper> Scores
    {
        get => this.scores ??= this.GetPropertyValueAsWrappers<DHScoreUnitWrapper>(keyScores);
        set
        {
            this.SetPropertyValueFromWrappers(keyScores, value);
            this.scores = value;
        }
    }

    public string EntityId => this.EntityType switch
    {
        RowScoreEntityType.DataProduct => this.EntityPayload.Value<string>(DQOutputFields.DP_ID)!,
        _ => throw new NotImplementedException($"EntityId for {this.EntityType} is not implemented")
    };
}