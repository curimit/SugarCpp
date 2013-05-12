using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.StringTemplate;

namespace SugarCpp.Compiler
{
    public enum ForItemType
    {
        Each, When, To, DownTo
    }

    public abstract class ForItem
    {
        public abstract ForItemType Type { get; }
    }

    public class ForItemEach : ForItem
    {
        public string Var;
        public Expr Expr;

        public override ForItemType Type
        {
            get { return ForItemType.Each; }
        }

        public ForItemEach(string var, Expr expr)
        {
            this.Var = var;
            this.Expr = expr;
        }
    }

    public class ForItemWhen : ForItem
    {
        public Expr Expr;

        public override ForItemType Type
        {
            get { return ForItemType.When; }
        }

        public ForItemWhen(Expr expr)
        {
            this.Expr = expr;
        }
    }

    public class ForItemTo : ForItem
    {
        public string Var;
        public Expr From;
        public Expr To;
        public Expr By;

        public override ForItemType Type
        {
            get { return ForItemType.To; }
        }

        public ForItemTo(string var, Expr from, Expr to, Expr by)
        {
            this.Var = var;
            this.From = from;
            this.To = to;
            this.By = by;
        }
    }

    public class ForItemDownTo : ForItem
    {
        public string Var;
        public Expr From;
        public Expr To;
        public Expr By;

        public override ForItemType Type
        {
            get { return ForItemType.DownTo; }
        }

        public ForItemDownTo(string var, Expr from, Expr to, Expr by)
        {
            this.Var = var;
            this.From = from;
            this.To = to;
            this.By = by;
        }
    }

    public class StmtFor : Stmt
    {
        public List<ForItem> List = new List<ForItem>();
        public StmtBlock Body;

        public StmtFor(List<ForItem> list, StmtBlock body)
        {
            if (list != null)
            {
                this.List = list;
            }
            this.Body = body;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
