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
        public List<Expr> ExprList = new List<Expr>();
        public bool IsEqualSign;

        public GlobalAlloc(string type, List<string> name, List<Expr> expr_list, List<Attr> attr, bool isEqualSign)
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
            this.IsEqualSign = isEqualSign;
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
