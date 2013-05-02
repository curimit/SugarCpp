using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Enum : AttrAstNode
    {
        public string Name;
        public List<string> Values = new List<string>();

        public Enum(string name, List<string> values, List<Attr> attr)
        {
            this.Name = name;
            if (values != null)
            {
                this.Values = values;
            }
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
