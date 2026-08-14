using System.Globalization;
using Convergex.Domain.Enums;

namespace Convergex.Application.Common;

public static class DecimalFormatter
{
    public static decimal Round(decimal value, int decimals, RoundingMode roundingMode)
    {
        if (roundingMode == RoundingMode.Truncate)
        {
            var factor = (decimal)Math.Pow(10, decimals);
            return Math.Truncate(value * factor) / factor;
        }

        return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
    }

    public static string Format(decimal value, int decimals, RoundingMode roundingMode)
        => Round(value, decimals, roundingMode).ToString("N" + decimals, CultureInfo.InvariantCulture);
}
