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
        public List<string> VarList = new List<string>();

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
