using System.Collections.Generic;
using System.Threading;

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

        public System.Threading.Tasks.Task<object> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Evaluator.ExecuteAsync(this, cancellationToken);
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

        public List<VariableInfo> GetAllVariablesInfo()
        {
            var vars = new List<VariableInfo>();
            TraverseGetVariables(Root, vars);
            return vars;
        }

        private static void TraverseGetVariables(Token token, List<VariableInfo> variables)
        {
            switch (token.Type)
            {
                case TokenType.Var:
                    variables.Add(new VariableInfo { Name = token.Value, Position = token.Position });
                    break;

                case TokenType.Group:
                    foreach (var t in token.Tokens)
                    {
                        TraverseGetVariables(t, variables);
                    }
                    break;

                case TokenType.Call:
                    foreach (var t in token.Arguments)
                    {
                        TraverseGetVariables(t, variables);
                    }
                    break;
            }
        }
    }
}
