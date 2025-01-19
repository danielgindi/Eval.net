using System;
using System.Collections.Generic;
using System.Threading;

namespace Eval.net
{
    public partial class EvalConfiguration
    {
        public delegate object EvalFunctionDelegate(EvalConfiguration config, params object[] args);
        public delegate System.Threading.Tasks.Task<object> AsyncEvalFunctionDelegate(CancellationToken cancellationToken, EvalConfiguration config, params object[] args);

        /// <summary>
        /// Explicitly return <see cref="Evaluator.ConstProviderDefault"/> to fallback
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public delegate object ConstProviderDelegate(string varname);
        public delegate System.Threading.Tasks.Task<object> AsyncConstProviderDelegate(CancellationToken cancellationToken, string varname);

        public static readonly EvalConfiguration FloatConfiguration = new EvalConfiguration(typeof(float));
        public static readonly EvalConfiguration DoubleConfiguration = new EvalConfiguration(typeof(double));
        public static readonly EvalConfiguration DecimalConfiguration = new EvalConfiguration(typeof(decimal));

        public Type NumericType { get; private set; }

        internal string[] _AllOperators;

        private string[][] _OperatorOrder;
        public string[][] OperatorOrder
        {
            get { return _OperatorOrder; }
            set
            {
                _OperatorOrder = value;

                var ops = new List<string>();
                foreach (var ops2 in _OperatorOrder)
                {
                    ops.AddRange(ops2);
                }
                _AllOperators = ops.ToArray();
            }
        }
        
        public HashSet<string> PrefixOperators { get; set; }
        public HashSet<string> SuffixOperators { get; set; }

        // https://en.wikipedia.org/wiki/Operator_associativity
        public HashSet<string> RightAssociativeOps { get; set; }

        public HashSet<char> VarNameChars { get; set; }

        public Dictionary<string, object> GenericConstants { get; set; }
        public Dictionary<string, EvalFunctionDelegate> GenericFunctions { get; set; }
        public Dictionary<string, object> Constants { get; set; }
        public Dictionary<string, EvalFunctionDelegate> Functions { get; set; }
        public Dictionary<string, AsyncEvalFunctionDelegate> AsyncFunctions { get; set; }

        /// <summary>
        /// Explicitly return <see cref="Evaluator.ConstProviderDefault"/> to fallback
        /// </summary>
        public ConstProviderDelegate ConstProvider { get; set; }
        public AsyncConstProviderDelegate AsyncConstProvider { get; set; }

        public bool AutoParseNumericStrings { get; set; } = true;
        public IFormatProvider AutoParseNumericStringsFormatProvider { get; set; } = null;

        public void SetConstant(string name, object value)
        {
            if (Constants == null)
            {
                Constants = new Dictionary<string, object>();
            }

            Constants[name] = value;
        }

        public void RemoveConstant(string name)
        {
            if (Constants == null) return;

            Constants.Remove(name);
        }

        public void SetFunction(string name, EvalFunctionDelegate func)
        {
            if (Functions == null)
            {
                Functions = new Dictionary<string, EvalFunctionDelegate>();
            }

            Functions[name] = func;
        }

        public void SetFunction(string name, AsyncEvalFunctionDelegate func)
        {
            if (AsyncFunctions == null)
            {
                AsyncFunctions = new Dictionary<string, AsyncEvalFunctionDelegate>();
            }

            AsyncFunctions[name] = func;
            Functions?.Remove(name);
        }

        public void RemoveFunction(string name)
        {
            if (Functions == null) return;

            Functions.Remove(name);
        }

        public void ClearConstants()
        {
            Constants.Clear();
        }

        public void ClearFunctions()
        {
            Functions.Clear();
            AsyncFunctions.Clear();
        }

        public EvalConfiguration() : this(typeof(double))
        {
        }

        public EvalConfiguration(
            Type numericType, 
            bool autoParseNumericStrings = true, 
            IFormatProvider autoParseNumericStringsFormatProvider = null)
        {
            this.NumericType = numericType;
            this.AutoParseNumericStrings = autoParseNumericStrings;
            this.AutoParseNumericStringsFormatProvider = autoParseNumericStringsFormatProvider;

            OperatorOrder = DefaultOperatorOrder;
            PrefixOperators = DefaultPrefixOperators;
            SuffixOperators = DefaultSuffixOperators;
            RightAssociativeOps = DefaultRightAssociativeOps;
            VarNameChars = DefaultVarNameChars;
            GenericConstants = new Dictionary<string, object>(DefaultGenericConstants);
            GenericFunctions = new Dictionary<string, EvalFunctionDelegate>(
                GetDefaultGenericFunctions(NumericType, AutoParseNumericStrings, AutoParseNumericStringsFormatProvider));
            Constants = new Dictionary<string, object>();
            Functions = new Dictionary<string, EvalFunctionDelegate>();
            AsyncFunctions = new Dictionary<string, AsyncEvalFunctionDelegate>();
        }

        public virtual EvalConfiguration Clone(bool deep = false)
        {
            return Clone<EvalConfiguration>(deep);
        }

        public T Clone<T>(bool deep = false) where T : EvalConfiguration, new()
        {
            var config = new T();
            config.NumericType = NumericType;
            config.OperatorOrder = OperatorOrder;
            config.PrefixOperators = deep ? new HashSet<string>(PrefixOperators) : PrefixOperators;
            config.SuffixOperators = deep ? new HashSet<string>(SuffixOperators) : SuffixOperators;
            config.RightAssociativeOps = deep ? new HashSet<string>(RightAssociativeOps) : RightAssociativeOps;
            config.VarNameChars = deep ? new HashSet<char>(VarNameChars) : VarNameChars;
            config.GenericConstants = deep ? new Dictionary<string, object>(GenericConstants) : GenericConstants;
            config.GenericFunctions = deep ? new Dictionary<string, EvalFunctionDelegate>(GenericFunctions) : GenericFunctions;
            config.Constants = deep ? new Dictionary<string, object>(Constants) : Constants;
            config.Functions = deep ? new Dictionary<string, EvalFunctionDelegate>(Functions) : Functions;
            config.AsyncFunctions = deep ? new Dictionary<string, AsyncEvalFunctionDelegate>(AsyncFunctions) : AsyncFunctions;
            config.ConstProvider = ConstProvider;
            config.AsyncConstProvider = AsyncConstProvider;
            return config;
        }
    }
}
