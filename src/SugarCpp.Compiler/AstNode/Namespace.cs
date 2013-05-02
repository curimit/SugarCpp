using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Namespace : AttrAstNode
    {
        public string Name;
        public GlobalBlock Block;

        public Namespace(string name, GlobalBlock block)
        {
            this.Name = name;
            this.Block = block;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
