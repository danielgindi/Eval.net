using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.net
{
    internal class Token
    {
        public TokenType Type;
        public string Value;
        public int Position;
        public List<List<Token>> ArgumentsGroups;
        public List<Token> Arguments;
        public List<Token> Tokens;
        public Token Left;
        public Token Right;
    }
}
