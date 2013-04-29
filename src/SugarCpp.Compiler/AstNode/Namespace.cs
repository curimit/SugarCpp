using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Namespace : AstNode
    {
        public string Name;
        public List<AstNode> List = new List<AstNode>();

        public Namespace(string name, List<AstNode> list)
        {
            this.Name = name;
            if (list != null)
            {
                this.List = list;
            }
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
