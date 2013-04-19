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
}
