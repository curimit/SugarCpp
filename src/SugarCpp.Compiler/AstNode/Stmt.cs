using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public abstract class Stmt : AstNode
    {
    }

    public class StmtIf : Stmt
    {
        public Expr Condition;
        public StmtBlock Body;
        public StmtBlock Else;

        public StmtIf(Expr condition, StmtBlock body_block, StmtBlock else_block)
        {
            this.Condition = condition;
            this.Body = body_block;
            this.Else = else_block;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtWhile : Stmt
    {
        public Expr Condition;
        public StmtBlock Body;

        public StmtWhile(Expr condition, StmtBlock body)
        {
            this.Condition = condition;
            this.Body = body;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtFor : Stmt
    {
        public Expr Start;
        public Expr Condition;
        public Expr Next;
        public StmtBlock Body;

        public StmtFor(Expr start, Expr condition, Expr next, StmtBlock body)
        {
            this.Start = start;
            this.Condition = condition;
            this.Next = next;
            this.Body = body;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtForEach : Stmt
    {
        public Expr Var;
        public Expr Target;
        public StmtBlock Body;

        public StmtForEach(Expr expr, Expr target, StmtBlock body)
        {
            this.Var = expr;
            this.Target = target;
            this.Body = body;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtTry : Stmt
    {
        public StmtBlock Body;
        public Stmt Stmt;
        public StmtBlock Catch;

        public StmtTry(StmtBlock body_block, Stmt stmt, StmtBlock catch_block)
        {
            this.Body = body_block;
            this.Stmt = stmt;
            this.Catch = catch_block;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtSwitchItem : Stmt
    {
        public Expr Expr;
        public StmtBlock Block;

        public StmtSwitchItem(Expr expr, StmtBlock block)
        {
            this.Expr = expr;
            this.Block = block;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtSwitch : Stmt
    {
        public List<StmtSwitchItem> List = new List<StmtSwitchItem>();
        public Expr Expr;

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtUsing : Stmt
    {
        public List<string> List = new List<string>();

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtExpr : Stmt
    {
        public Stmt Stmt;

        public StmtExpr(Stmt stmt)
        {
            this.Stmt = stmt;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtTypeDef : Stmt
    {
        public string Type;
        public string Name;

        public StmtTypeDef(string type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtReturn : Stmt
    {
        public Expr Expr;

        public StmtReturn(Expr expr)
        {
            this.Expr = expr;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtDefer : Stmt
    {
        public Stmt Stmt;

        public StmtDefer(Stmt stmt)
        {
            this.Stmt = stmt;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StmtFinally : Stmt
    {
        public Stmt Stmt;

        public StmtFinally(Stmt stmt)
        {
            this.Stmt = stmt;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
