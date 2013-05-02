using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class GlobalBlock : AstNode
    {
        public List<AttrAstNode> List = new List<AttrAstNode>();

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class GlobalUsing : AttrAstNode
    {
        public List<string> List = new List<string>();

        public GlobalUsing(List<string> list)
        {
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

    public class GlobalAlloc : AttrAstNode
    {
        public List<string> Name = new List<string>();
        public string Type;
        public Expr Expr;

        public GlobalAlloc(string type, List<string> name, Expr expr, List<Attr> attr)
        {
            this.Type = type;
            this.Name = name;
            this.Expr = expr;
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

    public class GlobalTypeDef : AttrAstNode
    {
        public string Type;
        public string Name;

        public GlobalTypeDef(string type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
