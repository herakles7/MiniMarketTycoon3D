using System;

namespace MiniMarketTycoon.Utilities
{
    /// <summary>
    /// Utility class for formatting currency amounts with compact suffixes (K, M, B, etc.).
    /// Optimized for mobile idle tycoon games.
    /// </summary>
    public static class CurrencyFormatter
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac", "ad" };

        public static string Format(double value, string prefix = "$")
        {
            if (value < 0)
            {
                return $"-{Format(-value, prefix)}";
            }

            if (value < 1000)
            {
                return $"{prefix}{Math.Floor(value):N0}";
            }

            int exponent = (int)(Math.Log10(value) / 3);
            if (exponent >= Suffixes.Length)
            {
                exponent = Suffixes.Length - 1;
            }

            double scaled = value / Math.Pow(10, exponent * 3);
            return $"{prefix}{scaled:F1}{Suffixes[exponent]}";
        }
    }
}
