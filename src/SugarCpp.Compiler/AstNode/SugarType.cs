using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.StringTemplate;

namespace SugarCpp.Compiler
{
    public abstract class SugarType : AstNode
    {
    }

    public class AutoType : SugarType
    {
        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class DeclType : SugarType
    {
        public Expr Expr;

        public DeclType(Expr expr)
        {
            this.Expr = expr;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class IdentType : SugarType
    {
        public string Type;

        public IdentType(string type)
        {
            this.Type = type;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StarType : SugarType
    {
        public SugarType Type;

        public StarType(SugarType type)
        {
            this.Type = type;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class RefType : SugarType
    {
        public SugarType Type;

        public RefType(SugarType type)
        {
            this.Type = type;
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class TemplateType : SugarType
    {
        public SugarType Type;
        public List<SugarType> Args = new List<SugarType>();

        public TemplateType(SugarType type, List<SugarType> args)
        {
            this.Type = type;
            if (args != null)
            {
                this.Args = args;
            }
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ArrayType : SugarType
    {
        public SugarType Type;
        public List<Expr> Args = new List<Expr>();

        public ArrayType(SugarType type, List<Expr> args)
        {
            this.Type = type;
            if (args != null)
            {
                this.Args = args;
            }
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FuncType : SugarType
    {
        public List<SugarType> Args = new List<SugarType>();
        public SugarType Type;

        public FuncType(List<SugarType> args, SugarType type)
        {
            this.Type = type;
            if (args != null)
            {
                this.Args = args;
            }
        }

        public override Template Accept(Visitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
