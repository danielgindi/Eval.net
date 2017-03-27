using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.net
{
    internal enum TokenType
    {
        String,
        Var,
        Call,
        Group,
        Number,
        Op,
        LeftParen,
        RightParen,
        Comma,
    }
}
