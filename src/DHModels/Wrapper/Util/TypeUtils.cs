#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class TypeUtils
    {
        private const string StringType = "String";

        private static readonly TypeMapping[] SupportedTypeMappings = new TypeMapping[]
        {
            new TypeMapping()
            {
                CSharpTypes = new HashSet<Type> { typeof(string) },
                JsonValueTypes = new HashSet<JTokenType> { JTokenType.String },
            },
            new TypeMapping()
            {
                CSharpTypes = new HashSet<Type> { typeof(bool) },
                JsonValueTypes = new HashSet<JTokenType> { JTokenType.Boolean },
            },
            new TypeMapping()
            {
                CSharpTypes = new HashSet<Type> { typeof(int), typeof(short), typeof(long), typeof(decimal), typeof(float), typeof(double) },
                JsonValueTypes = new HashSet<JTokenType> { JTokenType.Integer },
            },
            new TypeMapping()
            {
                CSharpTypes = new HashSet<Type> { typeof(JObject) },
                JsonValueTypes = new HashSet<JTokenType> { JTokenType.Object },
            },
            new TypeMapping()
            {
                CSharpTypes = new HashSet<Type> { typeof(DateTime), typeof(DateTime?) },
                JsonValueTypes = new HashSet<JTokenType> { JTokenType.Date },
            },
            new TypeMapping()
            {
                CSharpTypes = new HashSet<Type> { typeof(double), typeof(float) },
                JsonValueTypes = new HashSet<JTokenType> { JTokenType.Float },
            },
        };

        internal class TypeMapping
        {
            public HashSet<Type> CSharpTypes;
            public HashSet<JTokenType> JsonValueTypes;
        }

        public static void IsAcceptableJsonValueType(Type expectedType, JToken jsonValue)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException(nameof(expectedType));
            }
            if (jsonValue == null)
            {
                throw new ArgumentNullException(nameof(jsonValue));
            }

            var jsonValueType = jsonValue.Type;

            bool isNullableType = expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullableType)
            {
                if (jsonValue.Type == JTokenType.Null)
                {
                    return;
                }
                expectedType = Nullable.GetUnderlyingType(expectedType);
            }

            if (expectedType == typeof(BaseEntityWrapper) || expectedType.IsSubclassOf(typeof(BaseEntityWrapper)))
            {
                if (jsonValue.Type == JTokenType.Object || jsonValue.Type == JTokenType.Null)
                {
                    return;
                }
                else
                {
                    throw new InvalidCastException($"Cannot convert from {jsonValueType} to object");
                }
            }

            if (expectedType.IsEnum)
            {
                if (Enum.IsDefined(expectedType, jsonValue.ToString()))
                {
                    return;
                }
                else
                {
                    throw new EntityValidationException(String.Format(
                        CultureInfo.InvariantCulture,
                        StringResources.ErrorMessageEnumPropertyValueNotValid,
                        jsonValue.ToString(),
                        jsonValue.Path,
                        String.Join(", ", Enum.GetNames(expectedType))));
                }
            }

            bool isExpectedEnumerable = expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            bool isJsonArray = jsonValueType == JTokenType.Array;

            if (isExpectedEnumerable && isJsonArray)
            {
                var itemType = expectedType.GetGenericArguments()[0];
                foreach (var item in jsonValue.Children())
                {
                    IsAcceptableJsonValueType(itemType, item);
                }
            }
            else if (isExpectedEnumerable && !isJsonArray)
            {
                throw new InvalidCastException($"Cannot convert from {jsonValueType} to array");
            }
            else if (!isExpectedEnumerable && isJsonArray)
            {
                throw new InvalidCastException($"Cannot convert from array to {expectedType.Name}");
            }
            else
            {
                bool supportedMapping = SupportedTypeMappings.Any(mapping => mapping.CSharpTypes.Contains(expectedType) && mapping.JsonValueTypes.Contains(jsonValueType));
                if (!supportedMapping)
                {
                    throw new InvalidCastException($"Cannot convert from {jsonValueType} to {expectedType.Name}");
                }
            }
        }

        public static bool IsComparable(string type1, string type2)
        {
            // Currently it is simple, need enhance when getting complex
            return type1 == type2;
        }

        public static bool IsStringType(string type)
        {
            return type == StringType;
        }

        public static (int, int) ExtractPrecisionAndScaleFromSparkDecimalType(string decimalTypeString)
        {
            var regex = new Regex(@"decimal\((\d+),\s*(\d+)\)", RegexOptions.IgnoreCase);
            var match = regex.Match(decimalTypeString);

            if (match.Success)
            {
                int precision = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                int scale = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                return (precision, scale);
            }
            else
            {
                throw new ArgumentException("Invalid decimal type string format");
            }
        }
    }
}
