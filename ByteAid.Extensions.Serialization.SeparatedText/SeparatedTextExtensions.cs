using System.Collections;
using System.Globalization;
using System.Text;

namespace ByteAid.Extensions.Serialization;

public static class SeparatedTextExtensions
{
    public static string SerializeAsSeparatedText<T>(this ISerializationHelper helper, T input)
    {
        Type elementType;
        IEnumerable enumerable;

        if (input is IEnumerable<T>)
        {
            elementType = typeof(T);
            enumerable = (IEnumerable)input;
        }
        else if (input is IEnumerable collection)
        {
            var genericArg = input.GetType().GetGenericArguments()[0];
            elementType = genericArg;
            enumerable = collection;
        }
        else
        {
            elementType = typeof(T);
            enumerable = new[] { input };
        }

        var ruleset = helper.Rulesets.FirstOrDefault(r => r.Target == elementType)
            ?? throw new InvalidOperationException($"No serialization ruleset found for type {elementType.Name}");

        var separatorRule = ruleset.Rules.OfType<SeparatorSerializationRule>().FirstOrDefault()
            ?? throw new InvalidOperationException($"No separator configuration found for type {elementType.Name}");

        var separator = separatorRule.GetSeparator();
        var rules = ruleset.Rules.OfType<PropertySerializationRule>().OrderBy(r => r.Position).ToList();
        var output = new StringBuilder();

        if (ruleset.Rules.OfType<HeaderSerializationRule>().FirstOrDefault()?.HasHeaders == true)
        {
            output.AppendLine(string.Join(separator, rules.Select(r => r.HeaderName ?? r.Property)));
        }

        var properties = elementType.GetProperties();

        foreach (var item in enumerable)
        {
            var values = new List<string>();
            foreach (var rule in rules)
            {
                var property = properties.FirstOrDefault(p => p.Name == rule.Property)
                    ?? throw new InvalidOperationException($"Property {rule.Property} not found on type {elementType.Name}");
                var value = property.GetValue(item)?.ToString() ?? string.Empty;
                values.Add(value);
            }
            output.AppendLine(string.Join(separator, values));
        }

        return output.ToString();
    }

    public static T DeserializeFromSeparatedText<T>(this ISerializationHelper helper, string input)
    {
        var lines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            throw new InvalidOperationException("Input is empty");
        }

        var ruleset = helper.Rulesets.FirstOrDefault(r => r.Target == typeof(T)) ?? throw new InvalidOperationException($"No serialization ruleset found for type {typeof(T).Name}");
        var separatorRule = ruleset.Rules.OfType<SeparatorSerializationRule>().FirstOrDefault() ?? throw new InvalidOperationException($"No separator configuration found for type {typeof(T).Name}");
        var separator = separatorRule.GetSeparator();
        var hasHeaders = ruleset.Rules.OfType<HeaderSerializationRule>().FirstOrDefault()?.HasHeaders == true;
        var dataLineIndex = hasHeaders ? 1 : 0;

        if (lines.Length <= dataLineIndex)
        {
            throw new InvalidOperationException("Input contains no data rows");
        }

        var values = lines[dataLineIndex].Split(separator[0]); // Using first char as separator since it's a single char

        var rules = ruleset.Rules.OfType<PropertySerializationRule>().OrderBy(r => r.Position).ToList();
        var obj = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();

        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];
            var property = properties.FirstOrDefault(p => p.Name == rule.Property) ?? throw new InvalidOperationException($"Property {rule.Property} not found on type {typeof(T).Name}");
            var convertedValue = ConvertValue(values[i], property.PropertyType);
            property.SetValue(obj, convertedValue);
        }

        return obj;
    }

    public static List<T> DeserializeFromSeparatedText<T>(this ISerializationHelper helper, IEnumerable<string> lines)
    {
        var ruleset = helper.Rulesets.FirstOrDefault(r => r.Target == typeof(T)) ?? throw new InvalidOperationException($"No serialization ruleset found for type {typeof(T).Name}");
        var separatorRule = ruleset.Rules.OfType<SeparatorSerializationRule>().FirstOrDefault() ?? throw new InvalidOperationException($"No separator configuration found for type {typeof(T).Name}");
        var separator = separatorRule.GetSeparator();
        var rules = ruleset.Rules.OfType<PropertySerializationRule>().OrderBy(r => r.Position).ToList();
        var hasHeaders = ruleset.Rules.OfType<HeaderSerializationRule>().FirstOrDefault()?.HasHeaders == true;
        var startIndex = hasHeaders ? 1 : 0;

        var collection = new List<T>();
        var linesArray = lines.ToArray();

        for (int i = startIndex; i < linesArray.Length; i++)
        {
            var values = linesArray[i].Split(separator[0]);

            var obj = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            for (int j = 0; j < rules.Count; j++)
            {
                var rule = rules[j];
                var property = properties.FirstOrDefault(p => p.Name == rule.Property) ?? throw new InvalidOperationException($"Property {rule.Property} not found on type {typeof(T).Name}");
                var convertedValue = ConvertValue(values[j], property.PropertyType);
                property.SetValue(obj, convertedValue);
            }

            collection.Add(obj);
        }

        return collection;
    }

    public static string[] GetHeadersArray<T>(this ISerializationHelper helper)
    {
        var ruleset = helper.Rulesets.FirstOrDefault(r => r.Target == typeof(T))
            ?? throw new InvalidOperationException($"No serialization ruleset found for type {typeof(T).Name}");

        return ruleset.Rules
            .OfType<PropertySerializationRule>()
            .OrderBy(r => r.Position)
            .Select(r => r.HeaderName ?? r.Property)
            .ToArray();
    }

    public static string GetHeaderLine<T>(this ISerializationHelper helper)
    {
        var ruleset = helper.Rulesets.FirstOrDefault(r => r.Target == typeof(T))
            ?? throw new InvalidOperationException($"No serialization ruleset found for type {typeof(T).Name}");

        var separatorRule = ruleset.Rules.OfType<SeparatorSerializationRule>().FirstOrDefault()
            ?? throw new InvalidOperationException($"No separator configuration found for type {typeof(T).Name}");

        var separator = separatorRule.GetSeparator();

        var headers = ruleset.Rules
            .OfType<PropertySerializationRule>()
            .OrderBy(r => r.Position)
            .Select(r => r.HeaderName ?? r.Property);

        return string.Join(separator, headers);
    }

    private static object ConvertValue(string value, Type targetType)
    {
        // Handle empty values
        if (string.IsNullOrWhiteSpace(value))
        {
            // Check if type is nullable
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return null!;
            }

            // For value types, throw an exception if the value is required
            if (targetType.IsValueType)
            {
                throw new InvalidOperationException($"Cannot convert empty value to non-nullable type {targetType.Name}");
            }

            return null!;
        }

        // Handle nullable types
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            targetType = Nullable.GetUnderlyingType(targetType)!;
        }

        // String type
        if (targetType == typeof(string)) return value;

        // Handle enums
        if (targetType.IsEnum)
        {
            // Try parse with ignore case
            return Enum.Parse(targetType, value, true);
        }

        // Handle common value types
        if (targetType == typeof(bool))
        {
            if (bool.TryParse(value, out bool boolResult))
                return boolResult;

            // Handle additional boolean representations
            value = value.ToLowerInvariant();
            return value is "1" or "yes" or "y" or "true" or "on";
        }

        // Handle different date formats
        if (targetType == typeof(DateTime))
        {
            // Try specific formats before general parse
            string[] formats = [
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy",
            "MM-dd-yyyy"
        ];

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime dateResult))
            {
                return dateResult;
            }

            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }

#if NET8_0_OR_GREATER
        if (targetType == typeof(DateOnly))
        {
            return DateOnly.FromDateTime(DateTime.Parse(value, CultureInfo.InvariantCulture));
        }

        if (targetType == typeof(TimeOnly))
        {
            return TimeOnly.FromDateTime(DateTime.Parse(value, CultureInfo.InvariantCulture));
        }
#endif
        if (targetType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
        }

        // Handle TimeSpan
        if (targetType == typeof(TimeSpan))
        {
            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        // Handle Guid
        if (targetType == typeof(Guid))
        {
            return Guid.Parse(value);
        }

        // Handle numeric types
        if (targetType == typeof(int))
            return int.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(long))
            return long.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(short))
            return short.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(byte))
            return byte.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(sbyte))
            return sbyte.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(uint))
            return uint.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(ulong))
            return ulong.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(ushort))
            return ushort.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(decimal))
            return decimal.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(double))
            return double.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(float))
            return float.Parse(value, CultureInfo.InvariantCulture);

        // Handle char
        if (targetType == typeof(char))
        {
            if (value.Length != 1)
                throw new FormatException($"Cannot convert string of length {value.Length} to char");
            return value[0];
        }

        // Handle byte array (Base64)
        if (targetType == typeof(byte[]))
        {
            return Convert.FromBase64String(value);
        }

        // Handle URI
        if (targetType == typeof(Uri))
        {
            return new Uri(value);
        }

        // Handle Version
        if (targetType == typeof(Version))
        {
            return Version.Parse(value);
        }

        // Handle IPAddress
        if (targetType == typeof(System.Net.IPAddress))
        {
            return System.Net.IPAddress.Parse(value);
        }

        throw new InvalidOperationException($"Unsupported type: {targetType.Name}");
    }
}