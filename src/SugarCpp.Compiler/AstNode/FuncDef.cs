using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class FuncDef : AstNode
    {
        public string Name;
        public string Type;
        public StmtBlock Block;

        public FuncDef(string name, string type, StmtBlock block)
        {
            this.Name = name;
            this.Type = type;
            this.Block = block;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
