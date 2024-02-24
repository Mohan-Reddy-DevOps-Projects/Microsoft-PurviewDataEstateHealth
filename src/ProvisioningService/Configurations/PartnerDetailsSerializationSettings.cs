// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;

using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Json serialization settings for partner details that convert the interfaces to their implementations.
/// </summary>
internal sealed class PartnerDetailsSerializationSettings : JsonSerializerSettings
{
    /// <summary>
    /// The default constructor.
    /// </summary>
    public PartnerDetailsSerializationSettings()
    {
        this.Converters = new List<JsonConverter> {
                new AbstractTypeJsonConverter<PartnerDetails, IPartnerDetails>(),
                new AbstractTypeJsonConverter<PartnerDetailsPerOperation, IPartnerDetailsPerOperation>()
            };
    }
}
