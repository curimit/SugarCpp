using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public static class Extensions
    {
        public static bool IsWildCard(this AstNode node)
        {
            if (!(node is ExprConst)) return false;
            var expr = (ExprConst)node;
            return expr.Type == ConstType.Ident && expr.Text == "_";
        }
    }
}
