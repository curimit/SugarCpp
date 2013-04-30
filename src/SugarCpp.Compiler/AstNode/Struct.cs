using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Struct : AstNode
    {
        public List<StructMember> List = new List<StructMember>();
        public string Name;

        public Struct(string name, List<StructMember> list)
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

    public class StructMember : AstNode
    {
        public HashSet<string> Attribute = new HashSet<string>();
        public AstNode Node;

        public StructMember(AstNode node, HashSet<string> set)
        {
            this.Node = node;
            if (set != null)
            {
                this.Attribute = set;
            }
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
