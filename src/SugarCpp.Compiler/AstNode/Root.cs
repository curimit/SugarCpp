using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class Root : AstNode
    {
        public List<FuncDef> FuncList = new List<FuncDef>();
    }
}
