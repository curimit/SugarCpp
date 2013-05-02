using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Class : AttrAstNode
    {
        public GlobalBlock Block;
        public string Name;

        public Class(string name, GlobalBlock block, List<Attr> attr)
        {
            this.Name = name;
            this.Block = block;
            if (attr != null)
            {
                this.Attribute = attr;
            }
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
