using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Eval.net
{
    public partial class EvalConfiguration
    {
        public virtual object ConvertToNumber(object value)
        {
            var type = NumericType;

            if (type.IsInstanceOfType(value))
                return value;

            if (value == null)
                return ConvertToNumber(0);
            
            if (value is string)
            {
                if (type.Equals(typeof(Decimal)))
                    return Decimal.Parse((string)value, AutoParseNumericStringsFormatProvider ?? CultureInfo.InvariantCulture);

                if (type.Equals(typeof(Double)))
                    return (double)StringConversion.OptionallyConvertStringToDouble(value, AutoParseNumericStringsFormatProvider);

                if (type.Equals(typeof(Single)))
                    return (float)(double)StringConversion.OptionallyConvertStringToDouble(value, AutoParseNumericStringsFormatProvider);

                if (type.Equals(typeof(Int64)))
                    return Int64.Parse((string)value, AutoParseNumericStringsFormatProvider ?? CultureInfo.InvariantCulture);

                if (type.Equals(typeof(Int32)))
                    return Int32.Parse((string)value, AutoParseNumericStringsFormatProvider ?? CultureInfo.InvariantCulture);
            }

            return Convert.ChangeType(value, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object FilterArg(object arg)
        {
            if (AutoParseNumericStrings)
            {
                return StringConversion.OptionallyConvertStringToDouble(arg, AutoParseNumericStringsFormatProvider);
            }

            return arg;
        }

        public virtual object Add(object a, object b)
        {
            if (a is string)
            {
                // do not convert them to numbers by mistake.
                // multiplication could be used to cast to numbers.
                return (string)a + (dynamic)b;
            }

            a = FilterArg(a);
            b = FilterArg(b);
            return ((dynamic)a + (dynamic)b);
        }

        public virtual object Subtract(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return ((dynamic)a - (dynamic)b);
        }

        public virtual object Multiply(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return ((dynamic)a * (dynamic)b);
        }

        public virtual object Divide(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return ((dynamic)a / (dynamic)b);
        }

        public virtual object Pow(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            var val = Math.Pow(Convert.ToDouble(a), Convert.ToDouble(b));
            return Convert.ChangeType(val, NumericType);
        }

        public virtual bool LessThan(object a, object b)
        {
            return ((IComparable)a).CompareTo(b) < 0;
        }

        public virtual bool LessThanOrEqualsTo(object a, object b)
        {
            return ((IComparable)a).CompareTo(b) <= 0;
        }

        public virtual bool GreaterThan(object a, object b)
        {
            return ((IComparable)a).CompareTo(b) > 0;
        }

        public virtual bool GreaterThanOrEqualsTo(object a, object b)
        {
            return ((IComparable)a).CompareTo(b) >= 0;
        }

        public virtual bool EqualsTo(object a, object b)
        {
            if (a == null && b == null) return true;
            if ((a == null) != (b == null)) return false;
            return Object.Equals(a, b);
        }

        public virtual bool NotEqualsTo(object a, object b)
        {
            return !EqualsTo(a, b);
        }

        public virtual bool IsTruthy(object a)
        {
            if (a == null) return false;

            if (a is string)
            {
                return ((string)(object)a).Length > 0;
            }

            if (a is bool)
            {
                return ((bool)(object)a);
            }

            if (a is System.Collections.ICollection)
            {
                return (a as System.Collections.ICollection).Count > 0;
            }

            if (a is Array)
            {
                return (a as Array).Length > 0;
            }

            return !a.Equals(0);
        }

        public virtual bool LogicalNot(object a)
        {
            return !IsTruthy(a);
        }

        public virtual object Factorial(object n)
        {
            n = FilterArg(n);

            object one = Convert.ChangeType(1, NumericType);
            var s = one;

            for (object i = Convert.ChangeType(2, NumericType);
                LessThanOrEqualsTo(i, n);
                i = Add(i, one))
            {
                s = Multiply(s, i);
            }

            return s;
        }

        public virtual object Mod(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return ((dynamic)a % (dynamic)b);
        }

        public virtual object BitShiftLeft(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return Convert.ChangeType((Int64)(dynamic)a << (int)(dynamic)b, NumericType);
        }

        public virtual object BitShiftRight(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return Convert.ChangeType((Int64)(dynamic)a >> (int)(dynamic)b, NumericType);
        }

        public virtual object BitAnd(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return Convert.ChangeType((Int64)(dynamic)a & (Int64)(dynamic)b, NumericType);
        }

        public virtual object BitXor(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return Convert.ChangeType((Int64)(dynamic)a ^ (Int64)(dynamic)b, NumericType);
        }

        public virtual object BitOr(object a, object b)
        {
            a = FilterArg(a);
            b = FilterArg(b);
            return Convert.ChangeType((Int64)(dynamic)a | (Int64)(dynamic)b, NumericType);
        }
    }
}
