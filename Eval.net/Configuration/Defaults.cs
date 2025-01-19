using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eval.net
{
    public partial class EvalConfiguration
    {
        /// <summary>
        /// Ordering of operators
        /// https://en.wikipedia.org/wiki/Order_of_operations#Programming_languages
        /// </summary>
        public static readonly string[][] DefaultOperatorOrder = new string[][] {
            new string[] { "!" },
            new string[] { "**" },
            new string[] { "\\", "/", "*", "%" },
            new string[] { "+", "-" },
            new string[] { "<<", ">>" },
            new string[] { "<", "<=", ">", ">=" },
            new string[] { "==", "=", "!=", "<>" },
            new string[] { "&" },
            new string[] { "^" },
            new string[] { "|" },
            new string[] { "&&" },
            new string[] { "||" }
        };

        public static readonly HashSet<string> DefaultPrefixOperators = new HashSet<string>
        {
            "!"
        };

        public static readonly HashSet<string> DefaultSuffixOperators = new HashSet<string>
        {
            "!"
        };

        public static readonly HashSet<string> DefaultRightAssociativeOps = new HashSet<string>(new string[]
        {
            "**"
        });

        public static readonly Dictionary<string, object> DefaultGenericConstants = new Dictionary<string, object>
        {
            { "PI", Math.PI },
            { "PI_2", Math.PI / 2.0 },
            { "LOG2E", Math.Log(Math.E, 2) },
            { "DEG", Math.PI / 180.0 },
            { "E", Math.E },
            { "INFINITY", Double.PositiveInfinity },
            { "NAN", Double.NaN },
            { "TRUE", true },
            { "FALSE", false },
        };

        public static Dictionary<string, EvalFunctionDelegate> GetDefaultGenericFunctions(
            Type type, bool autoParseNumericStrings = true, IFormatProvider autoParseNumericStringsFormatProvider = null)
        {
            Func<object, object> argFilter;

            if (autoParseNumericStrings)
            {
                if (type.Equals(typeof(decimal)))
                {
                    argFilter = arg =>
                    {
                        return StringConversion.OptionallyConvertStringToDecimal(arg, autoParseNumericStringsFormatProvider);
                    };
                }
                else
                {
                    argFilter = arg =>
                    {
                        return StringConversion.OptionallyConvertStringToDouble(arg, autoParseNumericStringsFormatProvider);
                    };
                }
            }
            else
            {
                argFilter = arg => arg;
            }

            return new Dictionary<string, EvalFunctionDelegate>
            {
                { "ABS", (_c, args) => Convert.ChangeType(Math.Abs((dynamic)argFilter(args[0])), type) },
                { "ACOS", (_c, args) => Convert.ChangeType(Math.Acos(Convert.ToDouble(argFilter(args[0]))), type) },
                { "ASIN", (_c, args) => Convert.ChangeType(Math.Asin(Convert.ToDouble(argFilter(args[0]))), type) },
                { "ATAN", (_c, args) => Convert.ChangeType(Math.Atan(Convert.ToDouble(argFilter(args[0]))), type) },
                { "ATAN2", (_c, args) => Convert.ChangeType(Math.Atan2(Convert.ToDouble(argFilter(args[0])), Convert.ToDouble(argFilter(args[1]))), type) },
                { "CEILING", (_c, args) => {
                    var arg = args[0];
                    if (arg is double)
                        return Convert.ChangeType(Math.Ceiling((double)arg), type);
                    if (arg is decimal)
                        return Convert.ChangeType(Math.Ceiling((decimal)arg), type);
                    return Convert.ChangeType(Math.Ceiling(Convert.ToDouble(argFilter(arg))), type);
                } },
                { "COS", (_c, args) => Convert.ChangeType(Math.Cos(Convert.ToDouble(argFilter(args[0]))), type) },
                { "COSH", (_c, args) => Convert.ChangeType(Math.Cosh(Convert.ToDouble(argFilter(args[0]))), type) },
                { "EXP", (_c, args) => Convert.ChangeType(Math.Exp(Convert.ToDouble(argFilter(args[0]))), type) },
                { "FLOOR", (_c, args) => {
                    var arg = args[0];
                    if (arg is double)
                        return Convert.ChangeType(Math.Floor((double)arg), type);
                    if (arg is decimal)
                        return Convert.ChangeType(Math.Floor((decimal)arg), type);
                    return Convert.ChangeType(Math.Floor(Convert.ToDouble(argFilter(arg))), type);
                } },
                { "LOG", (_c, args) => {
                    if (args.Length == 2)
                        return Convert.ChangeType(Math.Log(Convert.ToDouble(argFilter(args[0])), Convert.ToDouble(argFilter(args[1]))), type);
                    return Convert.ChangeType(Math.Log(Convert.ToDouble(argFilter(args[0]))), type);
                } },
                { "LOG2", (_c, args) => Convert.ChangeType(Math.Log(Convert.ToDouble(argFilter(args[0])), 2), type) },
                { "LOG10", (_c, args) => Convert.ChangeType(Math.Log10(Convert.ToDouble(argFilter(args[0]))), type) },
                { "MAX", (_c, args) => {
                    if (args.Length == 0) return null;
                    else
                    {
                        dynamic v = argFilter(args[0]);
                        if (v == null) return null;
                        for (var i = 0; i < args.Length; i++)
                        {
                            dynamic v2 = argFilter(args[i]);
                            if (v2 == null) return null;
                            if (v2.CompareTo(v) > 0)
                            {
                                v = v2;
                            }
                        }
                        return v;
                    }
                } },
                { "MIN", (_c, args) => {
                    if (args.Length == 0) return null;
                    else
                    {
                        dynamic v = argFilter(args[0]);
                        if (v == null) return null;
                        for (var i = 0; i < args.Length; i++)
                        {
                            dynamic v2 = argFilter(args[i]);
                            if (v2 == null) return null;
                            if (v2.CompareTo(v) < 0)
                            {
                                v = v2;
                            }
                        }
                        return v;
                    }
                } },
                { "POW", (_c, args) => Convert.ChangeType(Math.Pow(Convert.ToDouble(argFilter(args[0])), Convert.ToDouble(argFilter(args[1]))), type) },
                { "ROUND", (_c, args) => {
                    var arg = args[0];
                    if (arg is double)
                        return Convert.ChangeType(Math.Round((double)arg), type);
                    if (arg is decimal)
                        return Convert.ChangeType(Math.Round((decimal)arg), type);
                    return Convert.ChangeType(Math.Round(Convert.ToDouble(argFilter(arg))), type);
                } },
                { "SIGN", (_c, args) => Convert.ChangeType(Math.Sign((dynamic)argFilter(args[0])), type) },
                { "SIN", (_c, args) => Convert.ChangeType(Math.Sin(Convert.ToDouble(argFilter(args[0]))), type) },
                { "SINH", (_c, args) => Convert.ChangeType(Math.Sinh(Convert.ToDouble(argFilter(args[0]))), type) },
                { "SQRT", (_c, args) => Convert.ChangeType(Math.Sqrt(Convert.ToDouble(argFilter(args[0]))), type) },
                { "TAN", (_c, args) => Convert.ChangeType(Math.Tan(Convert.ToDouble(argFilter(args[0]))), type) },
                { "TANH", (_c, args) => Convert.ChangeType(Math.Tanh(Convert.ToDouble(argFilter(args[0]))), type) },
                { "TRUNCATE", (_c, args) => {
                    var arg = args[0];
                    if (arg is double)
                        return Convert.ChangeType(Math.Truncate((double)arg), type);
                    if (arg is decimal)
                        return Convert.ChangeType(Math.Truncate((decimal)arg), type);
                    return Convert.ChangeType(Math.Truncate(Convert.ToDouble(argFilter(arg))), type);
                } },
            };
        }

        public static readonly HashSet<char> DefaultVarNameChars =
            new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$".ToCharArray());
    }
}
