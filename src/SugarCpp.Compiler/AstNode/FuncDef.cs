using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class FuncDef : AstNode
    {
        public List<string> VarList = new List<string>();
    }
}
