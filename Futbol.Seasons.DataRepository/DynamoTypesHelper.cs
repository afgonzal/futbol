using System;
using System.Collections.Generic;

namespace Futbol.Seasons.DataRepository
{
    public static class DynamoTypesHelper
    {
        public static string Encode(this Guid guid)
        {
            var encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded.Replace("/", "_").Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        public static Guid Decode(this string value)
        {
            value = value.Replace("_", "/").Replace("-", "+");
            byte[] buffer = Convert.FromBase64String(value + "==");
            return new Guid(buffer);
        }

        public static long ToEpoch(this DateTime value)
        {
            return new DateTimeOffset(value).ToUnixTimeSeconds();
        }

        public static long ToEpoch(this DateTimeOffset value)
        {
            return value.ToUnixTimeSeconds();
        }

        public static DateTimeOffset FromEpoch(this long value)
        {
            return DateTimeOffset.FromUnixTimeSeconds(value);
        }

        public static Dictionary<string, short?> ParseHierarchy(this string value)
        {
            var values = value.Split("#", StringSplitOptions.RemoveEmptyEntries);
            var result = new Dictionary<string, short?>();
            if (values.Length > 0)
                result.Add("Key1", Int16.Parse(values[0]));

            if (values.Length > 1)
            {
                if (values[1] == new string('0', 5))
                    result.Add("Key2", null);
                else
                    result.Add("Key2", Int16.Parse(values[1]));
            }
            else
                result.Add("Key2", null);

            return result;
        }

        public static string ParseCompositeKey(this string key, int index)
        {
            var values = key.Split("#", StringSplitOptions.RemoveEmptyEntries);
            return values.Length <= index ? null : values[index];
        }

        public static T? ParseCompositeKey<T>(this string key, int index, bool missingAsNulls = true) where T : struct
        {
            var values = key.Split("#", StringSplitOptions.None);
            if (values.Length <= index)
            {
                if (!missingAsNulls)
                    throw new ArgumentException($"Error parsing CompositeKey {key}");
                return null;
            }

            if (string.IsNullOrEmpty(values[index]))
                return default(T?);
            return (T)Convert.ChangeType(values[index], typeof(T));
        }

        public static string ReplaceCompositeKeyValue(this string key, string newValue, int index)
        {
            if (string.IsNullOrEmpty(key))
                key = new string('#', index);

            var values = key.Split("#", StringSplitOptions.None);
            if (!string.IsNullOrEmpty(newValue) && values.Length < index)
                throw new ArgumentException($"Error manipulating CompositeKey {key}");

            if (!string.IsNullOrEmpty(newValue))
            {
                if (values.Length == index)
                    Array.Resize(ref values, index + 1);
            }

            if (index < values.Length)
                values[index] = newValue;

            if (string.IsNullOrEmpty(newValue) && values.Length == index + 1)
                Array.Resize(ref values, index);

            if (string.IsNullOrEmpty(newValue) && values.Length > index + 1)
                throw new ArgumentException($"Error removing key {index} from CompositeKey {key}");

            return string.Join('#', values);
        }
    }
}
