// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using Microsoft.Data.DeltaLake.Types;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Newtonsoft.Json;

/// <summary>
/// BaseEventHubEntityModel
/// </summary>
public abstract class BaseEventHubEntityModel
{
    /// <summary>
    /// Account Id.
    /// </summary>
    [JsonProperty("accountId")]
    public string AccountId { get; set; }

    /// <summary>
    /// Event Id.
    /// </summary>
    [JsonProperty("eventId")]
    public string EventId { get; set; }

    /// <summary>
    /// Correlation Id.
    /// </summary>
    [JsonProperty("correlationId")]
    public string EventCorrelationId { get; set; }

    /// <summary>
    /// Precise timestamp.
    /// </summary>
    [JsonProperty("preciseTimestamp")]
    public DateTime EventCreationTimestamp { get; set; }

    /// <summary>
    /// Get payload kind.
    /// </summary>
    /// <returns></returns>
    public abstract PayloadKind GetPayloadKind();

    /// <summary>
    /// Get schema definition.
    /// </summary>
    public abstract StructType GetSchemaDefinition();

    /// <summary>
    /// Get schema fields common to all.
    /// </summary>
    /// <returns></returns>
    protected StructField[] GetCommonSchemaFields()
    {
        StructField[] schemaFields = new[]
        {
            new StructField("AccountId", DataTypes.String, false),
            new StructField("EventId", DataTypes.String, false),
            new StructField("EventCorrelationId", DataTypes.String, false),
            new StructField("EventCreationTimestamp", DataTypes.Timestamp, false),
        };

        return schemaFields;
    }
}

/// <summary>
/// Custom Type JSON Converter adaptive to ParquetToSparkSchemaConverter.
/// </summary>
public class CustomTypeConverter<T> : JsonConverter
{
    /// <inheritdoc />
    public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        T target = serializer.Deserialize<T>(reader);

        if (target == null)
        {
            return null;
        }

        var result = target.ToJson();
        return result;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}
