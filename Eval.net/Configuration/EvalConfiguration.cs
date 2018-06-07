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
        public delegate object EvalFunctionDelegate(params object[] args);
        public delegate object ConstProviderDelegate(string varname);

        public static readonly EvalConfiguration FloatConfiguration = new EvalConfiguration(typeof(float));
        public static readonly EvalConfiguration DoubleConfiguration = new EvalConfiguration(typeof(double));
        public static readonly EvalConfiguration DecimalConfiguration = new EvalConfiguration(typeof(decimal));

        private Type _NumericType;
        public Type NumericType { get { return _NumericType; } }

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

        public ConstProviderDelegate ConstProvider { get; set; }

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
        }

        public EvalConfiguration(Type numericType)
        {
            _NumericType = numericType;

            OperatorOrder = DefaultOperatorOrder;
            PrefixOperators = DefaultPrefixOperators;
            SuffixOperators = DefaultSuffixOperators;
            RightAssociativeOps = DefaultRightAssociativeOps;
            VarNameChars = DefaultVarNameChars;
            GenericConstants = new Dictionary<string, object>(DefaultGenericConstants);
            GenericFunctions = new Dictionary<string, EvalFunctionDelegate>(GetDefaultGenericFunctions(_NumericType));
            Constants = new Dictionary<string, object>();
            Functions = new Dictionary<string, EvalFunctionDelegate>();
        }

        public EvalConfiguration Clone(bool deep = false)
        {
            var config = new EvalConfiguration(NumericType);
            config.OperatorOrder = OperatorOrder;
            config.PrefixOperators = deep ? new HashSet<string>(PrefixOperators) : PrefixOperators;
            config.SuffixOperators = deep ? new HashSet<string>(SuffixOperators) : SuffixOperators;
            config.RightAssociativeOps = deep ? new HashSet<string>(RightAssociativeOps) : RightAssociativeOps;
            config.VarNameChars = deep ? new HashSet<char>(VarNameChars) : VarNameChars;
            config.GenericConstants = deep ? new Dictionary<string, object>(GenericConstants) : GenericConstants;
            config.GenericFunctions = deep ? new Dictionary<string, EvalFunctionDelegate>(GenericFunctions) : GenericFunctions;
            config.Constants = deep ? new Dictionary<string, object>(Constants) : Constants;
            config.Functions = deep ? new Dictionary<string, EvalFunctionDelegate>(Functions) : Functions;
            config.ConstProvider = ConstProvider;
            return config;
        }
    }
}
