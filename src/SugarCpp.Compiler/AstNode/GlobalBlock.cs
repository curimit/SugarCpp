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

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public enum AllocType
    {
        Declare, Equal, Bracket, Array
    }

    public class GlobalAlloc : AttrAstNode
    {
        public List<string> Name = new List<string>();
        public SugarType Type;
        public List<Expr> ExprList = new List<Expr>();
        public AllocType Style;

        public GlobalAlloc(SugarType type, string name, Expr expr, List<Attr> attr, AllocType style)
        {
            this.Type = type;
            this.Name.Add(name);
            this.ExprList.Add(expr);
            if (attr != null)
            {
                this.Attribute = attr;
            }
            this.Style = style;
        }

        public GlobalAlloc(SugarType type, List<string> name, List<Expr> expr_list, List<Attr> attr, AllocType style)
        {
            this.Type = type;
            this.Name = name;
            if (expr_list != null)
            {
                this.ExprList = expr_list;
            }
            if (attr != null)
            {
                this.Attribute = attr;
            }
            this.Style = style;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class GlobalTypeDef : AttrAstNode
    {
        public SugarType Type;
        public string Name;

        public GlobalTypeDef(SugarType type, string name)
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
