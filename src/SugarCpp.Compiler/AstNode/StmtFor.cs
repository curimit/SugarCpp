using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.StringTemplate;

namespace SugarCpp.Compiler
{
    public enum ForItemType
    {
        Each, When, To, DownTo, Map
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

    public class ForItemMap : ForItem
    {
        public string Var;
        public Expr Expr;

        public override ForItemType Type
        {
            get { return ForItemType.Map; }
        }

        public ForItemMap(string var, Expr expr)
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

    public enum ForItemRangeType
    {
        To, DownTo, Til
    }

    public class ForItemRange : ForItem
    {
        public string Var;
        public Expr From;
        public Expr To;
        public Expr By;

        public ForItemRangeType Style;

        public override ForItemType Type
        {
            get { return ForItemType.To; }
        }

        public ForItemRange(string var, Expr from, Expr to, Expr by, ForItemRangeType style)
        {
            this.Var = var;
            this.From = from;
            this.To = to;
            this.By = by;
            this.Style = style;
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
