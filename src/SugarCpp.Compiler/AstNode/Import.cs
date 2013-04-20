using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Import : AstNode
    {
        public List<string> NameList = new List<string>();

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }

    }
}
