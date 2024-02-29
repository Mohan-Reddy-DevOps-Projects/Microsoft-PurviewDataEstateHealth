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

    private readonly MethodInfo methodCreateEntityWrapper;

    public CosmosWrapperSerializer()
    {
        // Configure the JsonSerializer according to your needs
        this.serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            // Add more settings here as needed
        };

        // Get the type of the static class containing the method
        var helperType = typeof(EntityWrapperHelper);

        // Find the method with the specified attribute
        this.methodCreateEntityWrapper = helperType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.IsGenericMethod
                && m.GetGenericArguments().Length == 1 // Ensure the method has one generic type parameter
                && m.GetParameters().Length == 1 // Ensure the method accepts one parameter
                && m.GetParameters()[0].ParameterType == typeof(JObject))
            ?? throw new InvalidOperationException("Can not find the method CreateEntityWrapper for CosmosSerializer to use!"); // Check the parameter type
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
                        var jObjects = jArray.OfType<JObject>();

                        object?[] objects;

                        if (elementType.IsSubclassOf(typeof(BaseEntityWrapper)))
                        {
                            objects = jObjects.Select(x => this.DeserializeEntity<object>(x, elementType)).ToArray();
                        }
                        else
                        {
                            objects = jObjects.Select(x => x.ToObject(elementType)).ToArray();
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

    private T DeserializeEntity<T>(JObject jObject)
    {
        return this.DeserializeEntity<T>(jObject, typeof(T));
    }

    private T DeserializeEntity<T>(JObject jObject, Type entityType)
    {
        var genericMethodToInvoke = this.methodCreateEntityWrapper.MakeGenericMethod(entityType);
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