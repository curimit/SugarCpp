using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Struct : AstNode
    {
        public List<AstNode> List = new List<AstNode>();
        public string Name;

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
