namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[EntityWrapper(DHScoreBaseWrapperDerivedTypes.DataProductScore, EntityCategory.Score)]
public class DHDataProductScoreWrapper(JObject jObject) : DHScoreBaseWrapper(jObject)
{
    private const string keyDataProductId = "dataProductId";
    private const string keyDataProductDomainId = "dataProductDomainId";
    private const string keyDataProductOwners = "dataProductOwners";

    public DHDataProductScoreWrapper() : this([])
    {
        this.Type = DHScoreBaseWrapperDerivedTypes.DataProductScore;
    }

    [EntityTypeProperty(keyDataProductId)]
    public string DataProductId
    {
        get => this.GetTypePropertyValue<string>(keyDataProductId);
        set => this.SetTypePropertyValue(keyDataProductId, value);
    }

    [EntityTypeProperty(keyDataProductDomainId)]
    public string DataProductDomainId
    {
        get => this.GetTypePropertyValue<string>(keyDataProductDomainId);
        set => this.SetTypePropertyValue(keyDataProductDomainId, value);
    }

    private IEnumerable<string>? dataProductOwners;

    [EntityTypeProperty(keyDataProductOwners)]
    public IEnumerable<string>? DataProductOwners
    {
        get => this.dataProductOwners ??= this.GetPropertyValues<string>(keyDataProductOwners);
        set
        {
            this.SetPropertyValue(keyDataProductOwners, value);
            this.dataProductOwners = value;
        }
    }
}
