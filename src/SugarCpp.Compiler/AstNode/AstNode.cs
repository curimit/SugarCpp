using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public abstract class AstNode
    {
        public abstract Template Accept(Visitor visitor);
    }

    public abstract class AttrAstNode : AstNode
    {
        public List<Attr> Attribute = new List<Attr>(); 
    }
}
