namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public class CosmosWrapperSerializer : CosmosSerializer
{
    private readonly JsonSerializer serializer;
    private readonly IDataEstateHealthRequestLogger logger;

    // Map static method as static method
    private static readonly MethodInfo methodCreateEntityWrapper = typeof(EntityWrapperHelper)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.IsGenericMethod
             && m.GetGenericArguments().Length == 1
             && m.GetParameters().Length == 1
             && m.GetParameters()[0].ParameterType == typeof(JObject))
        ?? throw new InvalidOperationException("Can not find the method CreateEntityWrapper for CosmosSerializer to use!");

    public CosmosWrapperSerializer(IDataEstateHealthRequestLogger logger)
    {
        // JsonSerializer is not guaranteed as thread-safe, so we need to create a new instance for each request
        this.serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            // Add more settings here as needed
        };

        this.logger = logger;
    }

    public override T FromStream<T>(Stream stream)
    {
        if (stream == null || stream.CanRead == false)
        {
#nullable disable
            return default;
#nullable enable
        }

        using (var sr = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            object? jsonObject;
            try
            {
                jsonObject = this.serializer.Deserialize(jsonTextReader);
            }
            catch (Exception e)
            {
                this.logger.LogWarning(
                    message: "Failed to deserialize the stream to JObject or JArray! The stream is not a valid JSON object!",
                    operationName: $"{nameof(CosmosWrapperSerializer)}#{nameof(FromStream)}",
                    exception: e
                );
                throw;
            }

            switch (jsonObject)
            {
                case JObject jObject:
                    var type = typeof(T);

                    if (type.IsSubclassOf(typeof(BaseEntityWrapper)))
                    {
                        return this.DeserializeEntity<T>(jObject);
                    }

#nullable disable
                    return jObject.ToObject<T>();
#nullable enable

                case JArray jArray:
                    type = typeof(T);
                    if (type.IsArray)
                    {
                        var elementType = type.GetElementType() ?? throw new InvalidOperationException("No element type for an array!");


                        if (elementType.IsSubclassOf(typeof(BaseEntityWrapper)))
                        {
                            var jObjects = jArray.OfType<JObject>();
                            var objects = jObjects.Select(x => this.DeserializeEntity<object>(x, elementType)).ToArray();

                            var typedArray = Array.CreateInstance(elementType, objects.Length);

                            for (int i = 0; i < objects.Length; i++)
                            {
                                // Convert each element as necessary and assign it to the new array
                                // This example assumes that a direct cast is sufficient
                                // If not, you might need additional conversion logic here
                                typedArray.SetValue(objects[i], i);
                            }

                            // Convert the array to type T
                            return (T)(object)typedArray;
                        }

#nullable disable
                        return jArray.ToObject<T>();
#nullable enable
                    }
                    this.logger.LogWarning(
                        message: $"The steam type is an array, but the target generic type is not an array!",
                        operationName: $"{nameof(CosmosWrapperSerializer)}#{nameof(FromStream)}"
                    );
                    throw new InvalidCastException("The type is not an array!");
                default:
                    this.logger.LogWarning(
                        message: $"The steam type ({jsonObject?.GetType().Name}) is not JObject or JArray!",
                        operationName: $"{nameof(CosmosWrapperSerializer)}#{nameof(FromStream)}"
                    );
                    throw new InvalidCastException("The type is not JObject or JArray!");
            }
        }
    }

    public override Stream ToStream<T>(T input)
    {
        try
        {
            var stream = new MemoryStream();

            // Convert the input object to JObject
            JObject jObject = JObject.FromObject(input, this.serializer);

            this.ValidateInputSteamJObject(jObject);

            // Modify the JObject here as needed
            if (input is JObjectBaseWrapper jObjectBaseWrapper)
            {
                jObject.Add(nameof(JObjectBaseWrapper.JObject), jObjectBaseWrapper.JObject);
            }

            using (var sw = new StreamWriter(stream, leaveOpen: true))
            using (var writer = new JsonTextWriter(sw))
            {
                this.serializer.Serialize(writer, jObject);
                writer.Flush();
                sw.Flush();
            }
            stream.Position = 0;
            return stream;
        }
        catch (Exception e)
        {
            this.logger.LogWarning(
                message: "Failed to serialize the input object to stream!",
                operationName: $"{nameof(CosmosWrapperSerializer)}#{nameof(ToStream)}",
                exception: e
            );
            throw;
        }
    }

    private T DeserializeEntity<T>(JObject jObject)
    {
        return this.DeserializeEntity<T>(jObject, typeof(T));
    }

    private T DeserializeEntity<T>(JObject jObject, Type entityType)
    {
        try
        {
            var genericMethodToInvoke = methodCreateEntityWrapper.MakeGenericMethod(entityType);
            var wrappedJObject = jObject.GetValue(nameof(JObjectBaseWrapper.JObject));
            if (wrappedJObject == null)
            {
#nullable disable
                return default;
#nullable enable
            }
#nullable disable
            // Invoke the generic method with the JObject parameter
            var entity = (T)genericMethodToInvoke.Invoke(null, [wrappedJObject]);
#nullable enable

            if (entity is IContainerEntityWrapper containerEntityWrapper)
            {
                containerEntityWrapper.TenantId = jObject.GetValue(nameof(IContainerEntityWrapper.TenantId))?.ToObject<string>();
                containerEntityWrapper.AccountId = jObject.GetValue(nameof(IContainerEntityWrapper.AccountId))?.ToObject<string>();
            }

#nullable disable
            return entity;
#nullable enable
        }
        catch (Exception e)
        {
            this.logger.LogWarning(
                message: "Failed to deserialize the JObject to the target entity!",
                operationName: $"{nameof(CosmosWrapperSerializer)}#{nameof(DeserializeEntity)}",
                exception: e
            );
            throw;
        }
    }

    private void ValidateInputSteamJObject(JObject jObject)
    {
        var tenantIdFieldName = nameof(IContainerEntityWrapper.TenantId);

        // Check if "TenantId" field exists
        if (!jObject.TryGetValue(tenantIdFieldName, out var tenantIdToken))
        {
            var error = $"The '{tenantIdFieldName}' field does not exist in the input stream!";
            this.logger.LogCritical(
                message: error
            );
            throw new ArgumentException(error);
        }

        // Check if the value is null
        if (tenantIdToken.Type == JTokenType.Null)
        {
            var error = $"The '{tenantIdFieldName}' field cannot be null in the input stream!";
            this.logger.LogCritical(
                message: error
            );
            throw new ArgumentException(error);
        }

        // Convert to string and check if it's a valid GUID
        var tenantIdStr = tenantIdToken.ToString();
        if (!Guid.TryParse(tenantIdStr, out var tenantId))
        {
            var error = $"The '{tenantIdFieldName}' field ({tenantIdStr}) is not a valid GUID in the input stream!";
            this.logger.LogCritical(
                message: error
            );
            throw new FormatException(error);
        }

        // Check if the GUID is empty
        if (tenantId == Guid.Empty)
        {
            var error = $"The '{tenantIdFieldName}' field cannot be an empty GUID in the input stream!";
            this.logger.LogCritical(
                message: error
            );
            throw new ArgumentException(error);
        }
    }
}