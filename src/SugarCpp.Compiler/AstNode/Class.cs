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
        public List<ExprAlloc> Args = new List<ExprAlloc>();
        public List<SugarType> GenericParameter = new List<SugarType>();
        public List<string> Inherit = new List<string>(); 
        public string Name;

        public Class(string name, List<SugarType> gp, List<ExprAlloc> args, List<string> inherit ,GlobalBlock block, List<Attr> attr)
        {
            this.Name = name;
            if (gp != null)
            {
                this.GenericParameter = gp;
            }
            if (args != null)
            {
                this.Args = args;
            }
            if (inherit != null)
            {
                this.Inherit = inherit;
            }
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
