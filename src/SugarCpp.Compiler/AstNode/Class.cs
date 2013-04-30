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
        public string Name;

        public Class(string name, List<ClassMember> list)
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

    public class ClassMember : AstNode
    {
        public HashSet<string> Attribute = new HashSet<string>();
        public AstNode Node;

        public ClassMember(AstNode node, HashSet<string> set)
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
