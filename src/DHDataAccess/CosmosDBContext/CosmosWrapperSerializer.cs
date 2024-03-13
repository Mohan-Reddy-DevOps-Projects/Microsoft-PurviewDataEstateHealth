namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using Microsoft.Azure.Cosmos;
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

    // Map static method as static method
    private static readonly MethodInfo methodCreateEntityWrapper = typeof(EntityWrapperHelper)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.IsGenericMethod
             && m.GetGenericArguments().Length == 1
             && m.GetParameters().Length == 1
             && m.GetParameters()[0].ParameterType == typeof(JObject))
        ?? throw new InvalidOperationException("Can not find the method CreateEntityWrapper for CosmosSerializer to use!");

    public CosmosWrapperSerializer()
    {
        // JsonSerializer is not guaranteed as thread-safe, so we need to create a new instance for each request
        this.serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            // Add more settings here as needed
        };
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


            var jsonObject = this.serializer.Deserialize(jsonTextReader);
            switch (jsonObject)
            {
                case JObject jObject:
                    var type = typeof(T);

                    if (type.IsSubclassOf(typeof(BaseEntityWrapper)))
                    {
                        return DeserializeEntity<T>(jObject);
                    }

#nullable disable
                    return jObject.ToObject<T>();
#nullable enable

                case JArray jArray:
                    type = typeof(T);
                    if (type.IsArray)
                    {
                        var elementType = type.GetElementType() ?? throw new InvalidOperationException("No element type for an array!");

                        object?[] objects;

                        if (elementType.IsSubclassOf(typeof(BaseEntityWrapper)))
                        {
                            var jObjects = jArray.OfType<JObject>();
                            objects = jObjects.Select(x => DeserializeEntity<object>(x, elementType)).ToArray();
                        }
                        else
                        {
                            objects = jArray.OfType<object>().Select(x => x is JObject jObject ? jObject.ToObject(elementType) : Convert.ChangeType(x, elementType)).ToArray();
                        }

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
                    throw new InvalidCastException("The type is not an array!");
                default:
                    throw new InvalidCastException("The type is not JObject or JArray!");
            }
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();

        // Convert the input object to JObject
        JObject jObject = JObject.FromObject(input, this.serializer);

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

    private static T DeserializeEntity<T>(JObject jObject)
    {
        return DeserializeEntity<T>(jObject, typeof(T));
    }

    private static T DeserializeEntity<T>(JObject jObject, Type entityType)
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
}