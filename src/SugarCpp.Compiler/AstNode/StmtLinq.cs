using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.StringTemplate;

namespace SugarCpp.Compiler
{
    public abstract class LinqItem
    {
        
    }

    public class LinqFrom : LinqItem
    {
        public string Var;
        public Expr Expr;

        public LinqFrom(string var, Expr expr)
        {
            this.Var = var;
            this.Expr = expr;
        }
    }

    public class LinqLet : LinqItem
    {
        public string Var;
        public Expr Expr;

        public LinqLet(string var, Expr expr)
        {
            this.Var = var;
            this.Expr = expr;
        }
    }

    public class LinqWhere : LinqItem
    {
        public Expr Expr;

        public LinqWhere(Expr expr)
        {
            this.Expr = expr;
        }
    }

    public class StmtLinq : Stmt
    {
        public List<LinqItem> List = new List<LinqItem>();
        public StmtBlock Block;

        public StmtLinq(List<LinqItem> list, StmtBlock block)
        {
            if (list != null)
            {
                this.List = list;
            }
            this.Block = block;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
