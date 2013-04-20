using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class FuncDef : AstNode
    {
        public string Type;
        public string Name;
        public StmtBlock Block;

        public FuncDef(string type, string name, StmtBlock block)
        {
            this.Type = type;
            this.Name = name;
            this.Block = block;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
