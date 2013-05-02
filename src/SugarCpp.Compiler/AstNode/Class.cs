using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Class : AstNode
    {
        public List<ClassMember> List = new List<ClassMember>();
        public List<Attr> Attribute = new List<Attr>();
        public string Name;

        public Class(string name, List<ClassMember> list, List<Attr> attr)
        {
            this.Name = name;
            if (list != null)
            {
                this.List = list;
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

    public class ClassMember : AstNode
    {
        public List<Attr> Attribute = new List<Attr>();
        public AstNode Node;

        public ClassMember(AstNode node, List<Attr> list)
        {
            this.Node = node;
            if (list != null)
            {
                this.Attribute = list;
            }
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
