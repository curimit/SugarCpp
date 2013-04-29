using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.StringTemplate;

namespace SugarCpp.Compiler
{
    public abstract class Match : Expr
    {
    }

    public class MatchTuple : Match
    {
        public List<Expr> ExprList = new List<Expr>();

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
