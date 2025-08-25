using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Eval.net
{
    public class StringConversion
    {
        private static CultureInfo COMMA_DECIMAL_CULTURE = CultureInfo.GetCultureInfo("es");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GuessNumberComma(string val)
        {
            var sval = val.Trim();
            var p1 = val.IndexOf('.');
            var p2 = p1 == -1 ? -1 : val.LastIndexOf('.');
            var c1 = sval.IndexOf(',');
            var c2 = c1 == -1 ? -1 : val.LastIndexOf(',');
            var hasSign = val.Length > 0 && (sval[0] == '-' || val[0] == '+');
            var lenNoSign = hasSign ? val.Length - 1 : val.Length;

            bool isCommaBased = false;

            if (c1 != -1 && p1 != -1)
            { // who's last?
                isCommaBased = c2 > p2;
            }
            else if (c1 != c2)
            { // two commas, must be thousands
                isCommaBased = false;
            }
            else if (p1 != p2)
            { // two periods, must be thousands
                isCommaBased = true;
            }
            else if (c2 != -1 && (lenNoSign > 7 || lenNoSign < 5))
            { // there is a comma, but it could not be thousands as there should be more than one
                isCommaBased = true;
            }
            else if (p2 != -1 && (lenNoSign > 7 || lenNoSign < 5))
            { // there is a period, but it could not be thousands as there should be more than one
                isCommaBased = false;
            }
            else if (c1 != -1 && c2 != sval.Length - 4)
            { // comma not in thousands position
                isCommaBased = true;
            }
            else if (p1 != -1 && p2 != sval.Length - 4)
            { // period not in thousands position
                isCommaBased = false;
            }

            return isCommaBased;
        }

        public static object? OptionallyConvertStringToDouble(object? val, IFormatProvider? formatProvider)
        {
            if (val is string)
            {
                var sval = (string)val;

                if (double.TryParse(
                    sval,
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    formatProvider ?? (GuessNumberComma(sval) ? COMMA_DECIMAL_CULTURE : CultureInfo.InvariantCulture),
                    out var result))
                    return result;

                return val;
            }
            else
            {
                return val;
            }
        }

        public static object? OptionallyConvertStringToDecimal(object? val, IFormatProvider? formatProvider)
        {
            if (val is string)
            {
                var sval = (string)val;

                if (decimal.TryParse(
                    sval,
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    formatProvider ?? (GuessNumberComma(sval) ? COMMA_DECIMAL_CULTURE : CultureInfo.InvariantCulture),
                    out var result))
                    return result;

                return val;
            }
            else
            {
                return val;
            }
        }
    }
}
