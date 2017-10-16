using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.net
{
    public class CompiledExpression
    {
        internal CompiledExpression()
        {

        }

        internal Token Root;
        public EvalConfiguration Configuration { get; set; }

        public object Execute()
        {
            return Evaluator.Execute(this);
        }

        public void SetConstant(string name, object value)
        {
            Configuration.SetConstant(name, value);
        }

        public void RemoveConstant(string name)
        {
            Configuration.RemoveConstant(name);
        }

        public void SetFunction(string name, EvalConfiguration.EvalFunctionDelegate func)
        {
            Configuration.SetFunction(name, func);
        }

        public void RemoveFunction(string name)
        {
            Configuration.RemoveFunction(name);
        }

        public void ClearConstants()
        {
            Configuration.ClearConstants();
        }

        public void ClearFunctions()
        {
            Configuration.ClearFunctions();
        }
    }
}
