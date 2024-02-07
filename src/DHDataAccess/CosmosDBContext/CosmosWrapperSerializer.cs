namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using Microsoft.Azure.Cosmos;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;

public class CosmosWrapperSerializer : CosmosSerializer
{
    public override T FromStream<T>(Stream stream)
    {
        // Convert the Stream to a JObject
        using var reader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(reader);
        var serializer = new Newtonsoft.Json.JsonSerializer();
        var jObject = serializer.Deserialize<JObject>(jsonReader);

        if (jObject == null)
        {
#nullable disable
            return default;
#nullable enable
        }

        var wrapperJObject = jObject.GetValue(nameof(JObjectBaseWrapper.JObject));

        if (wrapperJObject == null)
        {
#nullable disable
            return default;
#nullable enable
        }

        try
        {
            var type = typeof(T);
            var createMethod = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
#nullable disable
            return createMethod != null
                ? (T)createMethod.Invoke(null, new[] { wrapperJObject })
                : (T)Activator.CreateInstance(type, new[] { wrapperJObject });
#nullable enable
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        var jsonWriter = new JsonTextWriter(writer);
        var serializer = new JsonSerializer();
        serializer.Serialize(jsonWriter, input);
        jsonWriter.Flush(); // Make sure all data is written to the stream
        writer.Flush(); // Ensure all buffered data is written to the underlying stream

        stream.Position = 0; // Reset the stream position to the beginning
        return stream;
    }
}