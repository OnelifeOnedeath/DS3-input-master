using System;
using System.Collections.Generic;
using System.Linq;

namespace DS3InputMaster.Utilities
{
    /// <summary>
    /// Расширения для улучшения читаемости кода
    /// </summary>
    public static class Extensions
    {
        public static bool HasFlag(this Enum value, Enum flag)
        {
            if (value == null) return false;
            if (flag == null) return false;
            
            var valueLong = Convert.ToInt64(value);
            var flagLong = Convert.ToInt64(flag);
            
            return (valueLong & flagLong) == flagLong;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            return dictionary != null && dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static bool IsBetween(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static bool IsBetween(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static float Clamp(this float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(this int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Lerp(this float start, float end, float amount)
        {
            return start + (end - start) * amount.Clamp(0, 1);
        }

        public static bool SequenceEqualNullable<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null && second == null) return true;
            if (first == null || second == null) return false;
            return first.SequenceEqual(second);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) return;
            foreach (var item in source) action(item);
        }

        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static TimeSpan Multiply(this TimeSpan timeSpan, double multiplier)
        {
            return TimeSpan.FromTicks((long)(timeSpan.Ticks * multiplier));
        }

        public static bool IsCloseTo(this float value, float target, float tolerance = 0.001f)
        {
            return Math.Abs(value - target) <= tolerance;
        }
    }
}
