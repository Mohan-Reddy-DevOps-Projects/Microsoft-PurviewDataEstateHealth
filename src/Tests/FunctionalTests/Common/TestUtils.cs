namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class TestUtils
    {
        public static string GetWrappersTestFileFolder()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "TestFiles");
        }

        public static async Task<T> DeserializeAndValidateResponse<T>(
            HttpResponseMessage response,
            string[] requiredStrings = null,
            string[] forbiddenStrings = null)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            requiredStrings = requiredStrings ?? Array.Empty<string>();
            foreach (var str in requiredStrings)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    Assert.IsTrue(content.Contains(str));
                }
            }
            forbiddenStrings = forbiddenStrings ?? Array.Empty<string>();
            foreach (var str in forbiddenStrings)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    Assert.IsFalse(content.Contains(str));
                }
            }

            return JsonConvert.DeserializeObject<T>(content)!;
        }

        public static JToken GetResponseValue(JObject jObject, string name)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }
            return jObject.GetValue(name);
        }

        public static void ValidateTimeFormat(JObject jObject)
        {
            var systemData = GetResponseValue(jObject, "systemData");
            Assert.IsTrue(((string)systemData["createdAt"]).Contains('Z'));
            Assert.IsTrue(((string)systemData["lastModifiedAt"]).Contains('Z'));
        }

        public static void IsJObjectSubset(JObject subset, JObject largerObject)
        {
            if (subset == null)
            {
                throw new ArgumentNullException(nameof(subset));
            }

            if (largerObject == null)
            {
                throw new ArgumentNullException(nameof(largerObject));
            }

            foreach (var property in subset.Properties())
            {
                JToken largerObjectValue;
                if (!largerObject.TryGetValue(property.Name, out largerObjectValue))
                {
                    Assert.Fail($"Property {property.Name} not found in larger object");
                }

                Assert.IsTrue(JToken.DeepEquals(property.Value, largerObjectValue));
            }
        }

        public static string UpdateJsonObject(string json, List<(string, object)> updated)
        {
            if (updated == null)
            {
                return json;
            }

            var obj = JsonConvert.DeserializeObject<JObject>(json);
            foreach (var pair in updated)
            {
                obj[pair.Item1] = JToken.FromObject(pair.Item2);
            }
            return JsonConvert.SerializeObject(obj);
        }
    }
}
