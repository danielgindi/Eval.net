using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.net
{
    public class Evaluator
    {
        public static CompiledExpression Compile(string expression, EvalConfiguration configuration)
        {
            var tokens = TokenizeExpression(expression, configuration);

            // Compact +-
            for (int i = 1, len = tokens.Count; i < len; i++)
            {
                var token = tokens[i];
                var prevToken = tokens[i - 1];

                if (token.Type == TokenType.Op &&
                    (token.Value == "-" || token.Value == "+") &&
                    prevToken.Type == TokenType.Op &&
                    (prevToken.Value == "-" || prevToken.Value == "+"))
                {
                    if (prevToken.Value != "+")
                    {
                        if (token.Value == "-")
                        {
                            token.Value = "+";
                        }
                        else
                        {
                            token.Value = "-";
                        }
                    }

                    tokens.RemoveAt(i - 1);
                    i--;
                    len = tokens.Count;

                    continue;
                }

                if (token.Type == TokenType.Number &&
                    prevToken.Type == TokenType.Op &&
                    (prevToken.Value == "-" || prevToken.Value == "+") &&
                    ((i > 1 && tokens[i - 2].Type == TokenType.Op) || i == 1)
                    )
                {
                    if (prevToken.Value == "-")
                    {
                        token.Value = prevToken.Value + token.Value;
                    }
                    tokens.RemoveAt(i - 1);
                    i--;
                    len = tokens.Count;

                    continue;
                }
            }

            // Take care of groups (including function calls)
            for (int i = 0, len = tokens.Count; i < len; i++)
            {
                var token = tokens[i];

                if (token.Type == TokenType.LeftParen)
                {
                    GroupTokens(tokens, i);
                    len = tokens.Count;
                    i--;
                }
            }

            // Build the tree
            var tree = BuildTree(tokens, configuration);

            return new CompiledExpression { Root = tree, Configuration = configuration };
        }

        public static object Execute(string expression, EvalConfiguration configuration)
        {
            return Execute(Compile(expression, configuration));
        }

        public static object Execute(CompiledExpression expression)
        {
            return EvaluateToken(expression.Root, expression.Configuration);
        }

        internal static string OpAtPosition(string expression, int start, EvalConfiguration configuration)
        {
            string op = null;

            var allOperators = configuration._AllOperators;

            for (int j = 0, jlen = allOperators.Length; j < jlen; j++)
            {
                var item = allOperators[j];

                if (op != null && (op == item || item.Length <= op.Length))
                    continue;

                if (expression.Substring(start, item.Length) == item)
                {
                    op = item;
                }
            }

            return op;
        }

        internal static int IndexOfOpInTokens(List<Token> tokens, string op)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.Type == TokenType.Op && Object.Equals(token.Value, op))
                    return i;
            }

            return -1;
        }

        internal static int LastIndexOfOpInTokens(List<Token> tokens, string op)
        {
            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                var token = tokens[i];
                if (token.Type == TokenType.Op && Object.Equals(token.Value, op))
                    return i;
            }

            return -1;
        }

        internal static void LastIndexOfOpArray(List<Token> tokens, string[] ops, EvalConfiguration config, out int matchIndex, out string match)
        {
            var pos = -1;
            string bestMatch = null;

            for (var i = 0; i < ops.Length; i++)
            {
                var item = ops[i];
                int opIndex;

                if (config.RightAssociativeOps.Contains(item))
                {
                    opIndex = IndexOfOpInTokens(tokens, item);
                }
                else
                {
                    opIndex = LastIndexOfOpInTokens(tokens, item);
                }

                if (opIndex == -1)
                    continue;

                if (pos == -1 || opIndex > pos)
                {
                    pos = opIndex;
                    bestMatch = item;
                }
            }

            matchIndex = pos;
            match = bestMatch;
        }

        internal static string ParseString(string data, int startAt, bool strict, bool unquote, out int newIndex)
        {
            int i = startAt;
            int len = data.Length;

            string output = "";

            var quote = '\0';
            if (unquote)
            {
                quote = data[i++];
                if (quote != '\'' && quote != '\"')
                {
                    throw new FormatException("Not a string");
                }
            }

            for (; i < len; i++)
            {
                var c = data[i];

                if (c == '\\')
                {
                    if (i + 1 == data.Length)
                    {
                        throw new IndexOutOfRangeException("Invalid string. An escape character with no escapee encountered at index " + i);
                    }

                    c = data[i + 1];

                    // Take a step forward here
                    i++;

                    // Test escapee

                    if (c == '\\' ||
                        c == '\'' ||
                        c == '\"')
                    {
                        output += c;
                    }
                    else if (c == 'b')
                    {
                        output += '\b';
                    }
                    else if (c == 'f')
                    {
                        output += '\f';
                    }
                    else if (c == 'n')
                    {
                        output += '\n';
                    }
                    else if (c == 'r')
                    {
                        output += '\r';
                    }
                    else if (c == 't')
                    {
                        output += '\t';
                    }
                    else if (c == 'u' || c == 'x')
                    {
                        int uffff = 0;
                        int hexSize = c == 'u' ? 4 : 2;

                        for (int j = 0; j < hexSize; j += 1)
                        {
                            c = data[++i];

                            int hex;

                            if (c >= '0' && c <= '9')
                            {
                                hex = c - '0';
                            }
                            else if (c >= 'a' && c <= 'f')
                            {
                                hex = c - 'a' + 10;
                            }
                            else if (c >= 'A' && c <= 'F')
                            {
                                hex = c - 'A' + 10;
                            }
                            else
                            {
                                if (strict)
                                {
                                    throw new FormatException("Unexpected escape sequence at index " + (i - j - 2));
                                }
                                else
                                {
                                    i--;
                                    break;
                                }
                            }

                            uffff = uffff * 16 + hex;
                        }

                        output += (char)uffff;
                    }
                    else
                    {
                        if (strict)
                        {
                            throw new FormatException("Unexpected escape sequence at index " + (i - 1));
                        }
                        else
                        {
                            output += c;
                        }
                    }
                }
                else if (unquote && c == quote)
                {
                    newIndex = i + 1;
                    return output;
                }
                else
                {
                    output += c;
                }
            }

            if (unquote)
            {
                throw new FormatException("String must be quoted with matching single-quote (') or double-quote(\") characters.");
            }

            newIndex = i;
            return output;
        }

        internal static string ParseString(string data, int startAt = 0, bool strict = false, bool unquote = false)
        {
            int newIndex;
            return ParseString(data, startAt, strict, unquote, out newIndex);
        }

        internal static string ParseNumber(string data, int startAt, out int newIndex)
        {
            int i = startAt;
            int len = data.Length;

            var exp = 0;
            var dec = false;

            if (i >= len)
            {
                throw new FormatException("Can't parse token at " + i);
            }

            for (; i < len; i++)
            {
                var c = data[i];

                if (c >= '0' && c <= '9')
                {
                    if (exp == 1) break;
                    if (exp > 1) exp++;
                }
                else if (c == '.')
                {
                    if (dec || exp > 0) break;
                    dec = true;
                }
                else if (c == 'e')
                {
                    if (exp > 0) break;
                    exp = 1;
                }
                else if (exp == 1 && (c == '-' || c == '+'))
                {
                    exp = 2;
                }
                else
                {
                    break;
                }
            }

            if (i == startAt || exp == 1 || exp == 2)
            {
                throw new FormatException("Unexpected character at index " + i);
            }

            newIndex = i;
            return data.Substring(startAt, i - startAt);
        }

        internal static List<Token> TokenizeExpression(string expression, EvalConfiguration configuration)
        {
            var varNameChars = configuration.VarNameChars;

            var tokens = new List<Token>();

            int i = 0;

            for (int len = expression.Length; i < len; i++)
            {
                var c = expression[i];

                var isDigit = c >= '0' && c <= '9';
                
                if (isDigit || c == '.')
                {
                    // Starting a number
                    int nextIndex;
                    var parsedNumber = ParseNumber(expression, i, out nextIndex);
                    tokens.Add(new Token
                    {
                        Type = TokenType.Number,
                        Position = i,
                        Value = parsedNumber
                    });
                    i = nextIndex - 1;
                    continue;
                }
                
                var isVarChars = varNameChars.Contains(c);

                if (isVarChars)
                {
                    // Starting a variable name - can start only with A-Z_

                    var token = "";

                    while (i < len)
                    {
                        c = expression[i];
                        isVarChars = varNameChars.Contains(c);
                        if (!isVarChars) break;

                        token += c;
                        i++;
                    }
                    
                    tokens.Add(new Token
                    {
                        Type = TokenType.Var,
                        Position = i - token.Length,
                        Value = token
                    });

                    i--; // Step back to continue loop from correct place

                    continue;
                }

                if (c == '\'' || c == '\"')
                {
                    int nextIndex;
                    var parsedString = ParseString(expression, i, false, true, out nextIndex);
                    tokens.Add(new Token
                    {
                        Type = TokenType.String,
                        Position = i,
                        Value = parsedString

                    });
                    i = nextIndex - 1;
                    continue;
                }

                if (c == '(')
                {
                    tokens.Add(new Token
                    {
                        Type = TokenType.LeftParen,
                        Position = i
                    });
                    continue;
                }

                if (c == ')')
                {
                    tokens.Add(new Token
                    {
                        Type = TokenType.RightParen,
                        Position = i
                    });
                    continue;
                }

                if (c == ',')
                {
                    tokens.Add(new Token
                    {
                        Type = TokenType.Comma,
                        Position = i
                    });
                    continue;
                }

                if (c == ' ' || c == '\t' || c == '\f' || c == '\r' || c == '\n')
                {
                    // Whitespace, skip
                    continue;
                }

                var op = OpAtPosition(expression, i, configuration);
                if (op != null)
                {
                    tokens.Add(new Token
                    {
                        Type = TokenType.Op,
                        Position = i,
                        Value = op
                    });
                    i += op.Length - 1;
                    continue;
                }

                throw new FormatException("Unexpected token at index " + i);
            }

            return tokens;
        }

        internal static Token GroupTokens(List<Token> tokens, int startAt = 0)
        {
            var isFunc = startAt > 0 && tokens[startAt - 1].Type == TokenType.Var;

            var rootToken = tokens[isFunc ? startAt - 1 : startAt];

            List<List<Token>> groups = null;
            List<Token> sub = null;

            if (isFunc)
            {
                rootToken.Type = TokenType.Call;
                groups = rootToken.ArgumentsGroups = new List<List<Token>>();
                sub = new List<Token>();
            }
            else
            {
                rootToken.Type = TokenType.Group;
                sub = rootToken.Tokens = new List<Token>();
            }

            for (int i = startAt + 1, len = tokens.Count; i < len; i++)
            {
                var token = tokens[i];

                if (isFunc && token.Type == TokenType.Comma)
                {
                    sub = new List<Token>();
                    groups.Add(sub);
                    continue;
                }

                if (token.Type == TokenType.RightParen)
                {
                    if (isFunc)
                    {
                        tokens.RemoveRange(startAt, i - startAt + 1);
                    }
                    else
                    {
                        tokens.RemoveRange(startAt + 1, i - startAt);
                    }
                    return rootToken;
                }

                if (token.Type == TokenType.LeftParen)
                {
                    GroupTokens(tokens, i);
                    i--;
                    len = tokens.Count;
                    continue;
                }

                if (isFunc && groups.Count == 0)
                {
                    groups.Add(sub);
                }
                sub.Add(token);
            }

            throw new FormatException("Unmatched parenthesis for parenthesis at index " + tokens[startAt].Position);
        }

        internal static int IndexOf<T>(T[] array, T item) where T : class
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == item)
                    return i;
            }

            return -1;
        }

        internal static Token BuildTree(List<Token> tokens, EvalConfiguration configuration)
        {
            var order = configuration.OperatorOrder;
            int orderCount = order.Length;
            var prefixOps = configuration.PrefixOperators;
            var suffixOps = configuration.SuffixOperators;

            for (int i = orderCount - 1; i >= 0; i--)
            {
                var cs = order[i];

                int pos;
                string op;
                LastIndexOfOpArray(tokens, cs, configuration, out pos, out op);

                if (pos != -1)
                {
                    var token = tokens[pos];

                    List<Token> left;
                    List<Token> right;

                    if (prefixOps.Contains(op) || suffixOps.Contains(op))
                    {
                        left = null;
                        right = null;

                        if (prefixOps.Contains(op) && pos == 0)
                        {
                            right = tokens.GetRange(pos + 1, tokens.Count - (pos + 1));
                        }
                        else if (suffixOps.Contains(op) && pos > 0)
                        {
                            left = tokens.GetRange(0, pos);
                        }

                        if (left == null && right == null)
                        {
                            throw new Exception("Operator " + token.Value.ToString() + " is unexpected at index " + token.Position);
                        }
                    }
                    else
                    {
                        left = tokens.GetRange(0, pos);
                        right = tokens.GetRange(pos + 1, tokens.Count - (pos + 1));

                        if (left.Count == 0 && (op == "-" || op == "+"))
                        {
                            left = null;
                        }
                    }

                    if ((left != null && left.Count == 0) ||
                        (right != null && right.Count == 0))
                    {
                        throw new Exception("Invalid expression, missing operand");
                    }

                    if (left == null && op == "-")
                    {
                        left = new List<Token> { new Token { Type = TokenType.Number, Value = "0" } };
                    }
                    else if (left == null && op == "+")
                    {
                        return BuildTree(right, configuration);
                    }

                    if (left != null)
                        token.Left = BuildTree(left, configuration);

                    if (right != null)
                        token.Right = BuildTree(right, configuration);

                    return token;
                }
            }

            if (tokens.Count > 1)
            {
                throw new Exception("Invalid expression, missing operand or operator at " + tokens[1].Position);
            }

            if (tokens.Count == 0)
            {
                throw new Exception("Invalid expression, missing operand or operator.");
            }

            var singleToken = tokens[0];

            if (singleToken.Type == TokenType.Group)
            {
                singleToken = BuildTree(singleToken.Tokens, configuration);
            }
            else if (singleToken.Type == TokenType.Call)
            {
                singleToken.Arguments = new List<Token>();
                for (int a = 0, arglen = singleToken.ArgumentsGroups.Count; a < arglen; a++)
                {
                    if (singleToken.ArgumentsGroups[a].Count == 0)
                        singleToken.Arguments.Add(null);
                    else
                        singleToken.Arguments.Add(BuildTree(singleToken.ArgumentsGroups[a], configuration));
                }
            }
            else if (singleToken.Type == TokenType.Comma)
            {
                throw new Exception("Unexpected character at index " + singleToken.Position);
            }

            return singleToken;
        }

        internal static object EvaluateToken(Token token, EvalConfiguration configuration)
        {
            var value = token.Value;

            switch (token.Type)
            {
                case TokenType.String:
                    return value;

                case TokenType.Number:
                    return configuration.ConvertToNumber(value);

                case TokenType.Var:

                    if (configuration.ConstProvider != null)
                    {
                        var val = configuration.ConstProvider(value);
                        if (val != null)
                            return val;
                    }

                    if (configuration.Constants.ContainsKey(value))
                        return configuration.Constants[value];

                    if (configuration.Constants.ContainsKey(value.ToUpperInvariant()))
                        return configuration.Constants[value.ToUpperInvariant()];

                    if (configuration.GenericConstants.ContainsKey(value))
                        return configuration.GenericConstants[value];

                    if (configuration.GenericConstants.ContainsKey(value.ToUpperInvariant()))
                        return configuration.GenericConstants[value.ToUpperInvariant()];

                    return null;

                case TokenType.Call:
                    return EvaluateFunction(token, configuration);

                case TokenType.Op:
                    switch (token.Value)
                    {

                        case "!": // Factorial or Not
                            if (token.Left != null) // Factorial (i.e. 5!)
                            {
                                return configuration.Factorial(EvaluateToken(token.Right, configuration));
                            }
                            else // Not (i.e. !5)
                            {
                                return configuration.LogicalNot(EvaluateToken(token.Right, configuration));
                            }

                        case "/": // Divide
                        case "\\":
                            return configuration.Divide(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "*": // Multiply
                            return configuration.Multiply(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "+": // Add
                            return configuration.Add(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "-": // Subtract
                            return configuration.Subtract(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "<<": // Shift left
                            return configuration.BitShiftLeft(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case ">>": // Shift right
                            return configuration.BitShiftRight(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "<": // Less than
                            return configuration.LessThan(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "<=": // Less than or equals to
                            return configuration.LessThanOrEqualsTo(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case ">": // Greater than
                            return configuration.GreaterThan(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case ">=": // Greater than or equals to
                            return configuration.GreaterThanOrEqualsTo(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "==": // Equals to
                        case "=":
                            return configuration.EqualsTo(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "!=": // Not equals to
                        case "<>":
                            return configuration.NotEqualsTo(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "**": // Power
                            return configuration.Pow(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "%": // Mod
                            return configuration.Mod(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "&": // Bitwise AND
                            return configuration.BitAnd(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "^": // Bitwise XOR
                            return configuration.BitXor(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "|": // Bitwise OR
                            return configuration.BitOr(EvaluateToken(token.Left, configuration), EvaluateToken(token.Right, configuration));

                        case "&&": // Logical AND
                            {
                                var res = EvaluateToken(token.Left, configuration);
                                if (configuration.IsTruthy(res))
                                    return EvaluateToken(token.Right, configuration);
                                return res;
                            }

                        case "||": // Logical OR
                            {
                                var res = EvaluateToken(token.Left, configuration);
                                if (!configuration.IsTruthy(res))
                                    return EvaluateToken(token.Right, configuration);
                                return res;
                            }

                    }
                    break;
            }

            throw new Exception("An unexpected error occurred while evaluating expression");
        }

        internal static object EvaluateFunction(Token token, EvalConfiguration configuration)
        {
            var fname = token.Value;

            var args = new List<object>();
            for (var i = 0; i < token.Arguments.Count; i++)
            {
                if (token.Arguments[i] == null)
                {
                    args.Add(null);
                }
                else
                {
                    args.Add(EvaluateToken(token.Arguments[i], configuration));
                }
            }

            if (configuration.Functions.ContainsKey(fname))
                return configuration.Functions[fname](args.ToArray());

            if (configuration.Functions.ContainsKey(fname.ToUpperInvariant()))
                return configuration.Functions[fname.ToUpperInvariant()](args.ToArray());

            if (configuration.GenericFunctions.ContainsKey(fname))
                return configuration.GenericFunctions[fname](args.ToArray());

            if (configuration.GenericFunctions.ContainsKey(fname.ToUpperInvariant()))
                return configuration.GenericFunctions[fname.ToUpperInvariant()](args.ToArray());

            throw new Exception("Function named \"" + fname + "\" was not found");
        }
    }
}