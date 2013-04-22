using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class FuncDef : AstNode
    {
        public string Type;
        public List<Expr> Args = new List<Expr>();
        public List<string> GenericParameter = new List<string>();
        public string Name;
        public StmtBlock Body;

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
