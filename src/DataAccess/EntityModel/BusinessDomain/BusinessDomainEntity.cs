// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

internal class BusinessDomainEntity : BaseEntity
{
    public BusinessDomainEntity()
    {
    }

    public BusinessDomainEntity(BusinessDomainEntity entity)
    {
        this.BusinessDomainId = entity.BusinessDomainId;
        this.BusinessDomainName = entity.BusinessDomainName;
    }

    [JsonProperty("businessDomainName")]
    public string BusinessDomainName { get; set; }

    [JsonProperty("businessDomainId")]
    public Guid BusinessDomainId { get; set; }
}

