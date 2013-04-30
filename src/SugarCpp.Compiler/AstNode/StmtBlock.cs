using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{

    public class StmtBlock : AstNode
    {
        public List<Stmt> StmtList = new List<Stmt>();

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
