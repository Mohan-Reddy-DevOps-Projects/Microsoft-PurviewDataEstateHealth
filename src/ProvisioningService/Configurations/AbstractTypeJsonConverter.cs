// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;

using Newtonsoft.Json;
using System;

/// <summary>
/// The converter to use the implementation class when trying to serialize an interface or an abstract base class.
/// </summary>
/// <typeparam name="TRealType">The implementation of the abstract type.</typeparam>
/// <typeparam name="TAbstractType">The interface or an abstract type.</typeparam>
public class AbstractTypeJsonConverter<TRealType, TAbstractType> : JsonConverter where TRealType : TAbstractType
{
    /// <inheritdoc/>
    public override bool CanConvert(Type objectType)
        => objectType == typeof(TAbstractType);

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer jsonSerializer)
        => jsonSerializer.Deserialize<TRealType>(reader);

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer jsonSerializer)
        => jsonSerializer.Serialize(writer, value);
}
