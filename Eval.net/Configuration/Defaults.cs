using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public static Dictionary<string, EvalFunctionDelegate> GetDefaultGenericFunctions(Type type)
        {
            return new Dictionary<string, EvalFunctionDelegate>
            {
                { "ABS", args => Convert.ChangeType(Math.Abs((dynamic)args[0]), type) },
                { "ACOS", args => Convert.ChangeType(Math.Acos((double)(dynamic)args[0]), type) },
                { "ASIN", args => Convert.ChangeType(Math.Asin((double)(dynamic)args[0]), type) },
                { "ATAN", args => Convert.ChangeType(Math.Atan((double)(dynamic)args[0]), type) },
                { "ATAN2", args => Convert.ChangeType(Math.Atan2((double)(dynamic)args[0], (double)(dynamic)args[1]), type) },
                { "CEILING", args => Convert.ChangeType(Math.Ceiling((dynamic)args[0]), type) },
                { "COS", args => Convert.ChangeType(Math.Cos((double)(dynamic)args[0]), type) },
                { "COSH", args => Convert.ChangeType(Math.Cosh((double)(dynamic)args[0]), type) },
                { "EXP", args => Convert.ChangeType(Math.Exp((double)(dynamic)args[0]), type) },
                { "FLOOR", args => Convert.ChangeType(Math.Floor((dynamic)args[0]), type) },
                { "LOG", args => {
                    if (args.Length == 2)
                        return Convert.ChangeType(Math.Log((double)(dynamic)args[0], (double)(dynamic)args[1]), type);
                    return Convert.ChangeType(Math.Log((double)(dynamic)args[0]), type);
                } },
                { "LOG2", args => Convert.ChangeType(Math.Log((double)(dynamic)args[0], 2), type) },
                { "LOG10", args => Convert.ChangeType(Math.Log10((double)(dynamic)args[0]), type) },
                { "MAX", args => {
                    if (args.Length == 0) return null;
                    else
                    {
                        dynamic v = args[0];
                        if (v == null) return null;
                        for (var i = 0; i < args.Length; i++)
                        {
                            dynamic v2 = args[i];
                            if (v2 == null) return null;
                            if (v2.CompareTo(v) > 0)
                            {
                                v = v2;
                            }
                        }
                        return v;
                    }
                } },
                { "MIN", args => {
                    if (args.Length == 0) return null;
                    else
                    {
                        dynamic v = args[0];
                        if (v == null) return null;
                        for (var i = 0; i < args.Length; i++)
                        {
                            dynamic v2 = args[i];
                            if (v2 == null) return null;
                            if (v2.CompareTo(v) < 0)
                            {
                                v = v2;
                            }
                        }
                        return v;
                    }
                } },
                { "POW", args => Convert.ChangeType(Math.Pow((double)(dynamic)args[0], (double)(dynamic)args[1]), type) },
                { "ROUND", args => Convert.ChangeType(Math.Round((dynamic)args[0]), type) },
                { "SIGN", args => Convert.ChangeType(Math.Sign((dynamic)args[0]), type) },
                { "SIN", args => Convert.ChangeType(Math.Sin((double)(dynamic)args[0]), type) },
                { "SINH", args => Convert.ChangeType(Math.Sinh((double)(dynamic)args[0]), type) },
                { "SQRT", args => Convert.ChangeType(Math.Sqrt((double)(dynamic)args[0]), type) },
                { "TAN", args => Convert.ChangeType(Math.Tan((double)(dynamic)args[0]), type) },
                { "TANH", args => Convert.ChangeType(Math.Tanh((double)(dynamic)args[0]), type) },
                { "TRUNCATE", args => Convert.ChangeType(Math.Truncate((dynamic)args[0]), type) },
            };
        }

        public static readonly HashSet<char> DefaultVarNameChars =
            new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_$".ToCharArray());
    }
}
