using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Antlr4.StringTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarCpp.Compiler
{
    public class TargetCpp : Visitor
    {
        public string Compile(string input)
        {
            input = input.Replace("\r", "");
            ANTLRStringStream Input = new ANTLRStringStream(input);
            SugarCppLexer lexer = new SugarCppLexer(Input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);

            SugarCppParser parser = new SugarCppParser(tokens);

            AstParserRuleReturnScope<CommonTree, IToken> t = parser.root();
            CommonTree ct = (CommonTree)t.Tree;

            CommonTreeNodeStream nodes = new CommonTreeNodeStream(ct);
            SugarWalker walker = new SugarWalker(nodes);

            Root x = walker.root();
            return x.Accept(this).Render();
        }

        public override Template Visit(Root root)
        {
            Template template = new Template("<list; separator=\"\n\n\">");
            List<Template> list = new List<Template>();
            foreach (var node in root.List)
            {
                list.Add(node.Accept(this));
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(Enum enum_def)
        {
            Template template = new Template("enum <name> {\n    <list; separator=\",\n\">\n};");
            template.Add("name", enum_def.Name);
            template.Add("list", enum_def.Values);
            return template;
        }

        public override Template Visit(Import import)
        {
            Template template = new Template("<list; separator=\"\n\">");
            List<Template> list = new List<Template>();
            foreach (var name in import.NameList)
            {
                Template node = new Template("#include <name>");
                node.Add("name", name);
                list.Add(node);
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(Struct struct_def)
        {
            Template template = new Template("struct <name> {\n    <list; separator=\"\n\">\n};");
            template.Add("name", struct_def.Name);
            List<Template> list = new List<Template>();
            foreach (var node in struct_def.List)
            {
                Template member = new Template("<expr>;");
                member.Add("expr", node.Accept(this));
                list.Add(member);
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(FuncDef func_def)
        {
            if (func_def.GenericParameter.Count() == 0)
            {
                Template template = new Template("<type> <name>(<args; separator=\", \">) {\n    <list; separator=\"\n\">\n}");
                template.Add("type", func_def.Type);
                template.Add("name", func_def.Name);
                template.Add("args", func_def.Args.Select(x => x.Accept(this)));
                template.Add("list", func_def.Body.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("template \\<<generics; separator=\", \">>\n<type> <name>(<args; separator=\", \">) {\n    <list; separator=\"\n\">\n}");
                template.Add("type", func_def.Type);
                template.Add("name", func_def.Name);
                template.Add("args", func_def.Args.Select(x => x.Accept(this)));
                template.Add("list", func_def.Body.Accept(this));
                template.Add("generics", func_def.GenericParameter.Select(x => string.Format("typename {0}", x)));
                return template;
            }
        }

        public override Template Visit(StmtBlock block)
        {
            Template template = new Template("<list; separator=\"\n\">");
            List<Template> list = new List<Template>();
            foreach (var node in block.StmtList)
            {
                Template expr = new Template("<expr>;");
                expr.Add("expr", node.Accept(this));
                list.Add(expr);
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(StmtIf stmt_if)
        {
            if (stmt_if.Else == null)
            {
                Template template = new Template("if <cond> {\n    <body>\n}");
                template.Add("cond", stmt_if.Condition.Accept(this));
                template.Add("body", stmt_if.Body.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("if <cond> {\n    <body1>\n} else {\n    <body2>\n}");
                template.Add("cond", stmt_if.Condition.Accept(this));
                template.Add("body1", stmt_if.Body.Accept(this));
                template.Add("body2", stmt_if.Else.Accept(this));
                return template;
            }

        }

        public override Template Visit(StmtWhile stmt_while)
        {
            Template template = new Template("while <cond> {\n    <body>\n}");
            template.Add("cond", stmt_while.Condition.Accept(this));
            template.Add("body", stmt_while.Body.Accept(this));
            return template;
        }

        public override Template Visit(StmtFor stmt_for)
        {
            Template template = new Template("for (<start>; <cond>; <next>) {\n    <body>\n}");
            template.Add("start", stmt_for.Start.Accept(this));
            template.Add("cond", stmt_for.Condition.Accept(this));
            template.Add("next", stmt_for.Next.Accept(this));
            template.Add("body", stmt_for.Body.Accept(this));
            return template;
        }

        public override Template Visit(MatchTuple match)
        {
            Template template = new Template("std::tie(<list; separator=\",\">)");
            template.Add("list", match.VarList);
            return template;
        }

        public override Template Visit(ExprAssign expr)
        {
            Template template = new Template("(<left> = <right>)");
            template.Add("left", expr.Left.Accept(this));
            template.Add("right", expr.Right.Accept(this));
            return template;
        }

        public override Template Visit(ExprLambda expr)
        {
            Template template = new Template("([](<args; separator=\",\">) { <expr>; })");
            template.Add("args", expr.Args.Select(x => x.Accept(this)));
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprAlloc expr)
        {
            if (expr.Expr != null)
            {
                Template template = new Template("<type> <name> = <expr>");
                template.Add("type", expr.Type);
                template.Add("name", expr.Name);
                template.Add("expr", expr.Expr.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("<type> <name>");
                template.Add("type", expr.Type);
                template.Add("name", expr.Name);
                return template;
            }
        }

        public override Template Visit(ExprTuple expr)
        {
            Template template = new Template("std::make_tuple(<list; separator=\", \">)");
            template.Add("list", expr.ExprList.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprCall expr)
        {
            Template template = new Template("(<expr>(<args; separator=\", \">))");
            template.Add("expr", expr.Expr.Accept(this));
            template.Add("args", expr.Args.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprCond expr)
        {
            Template template = new Template("(<cond> ? <expr1> : <expr2>)");
            template.Add("cond", expr.Cond.Accept(this));
            template.Add("expr1", expr.Expr1.Accept(this));
            template.Add("expr2", expr.Expr2.Accept(this));
            return template;
        }

        public override Template Visit(ExprReturn expr)
        {
            if (expr.Expr != null)
            {
                Template template = new Template("return <expr>");
                template.Add("expr", expr.Expr.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("return");
                return template;
            }
        }

        public override Template Visit(ExprDict expr)
        {
            Template template = new Template("(<expr>[<index>])");
            template.Add("expr", expr.Expr.Accept(this));
            template.Add("index", expr.Index.Accept(this));
            return template;
        }

        public override Template Visit(ExprNew expr)
        {
            Template template = new Template("(new <elem><ranges>)");
            template.Add("elem", expr.ElemType);
            List<Template> list = new List<Template>();
            foreach (var node in expr.Ranges)
            {
                Template item = new Template("[<expr>]");
                item.Add("expr", node.Accept(this));
                list.Add(item);
            }
            template.Add("ranges", list);
            return template;
        }

        public override Template Visit(ExprDot expr)
        {
            Template template = new Template("(<expr>.<name>)");
            template.Add("expr", expr.Expr.Accept(this));
            template.Add("name", expr.Name);
            return template;
        }

        public override Template Visit(ExprPrefix expr)
        {
            Template template = new Template("(<op><expr>)");
            template.Add("op", expr.Op);
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprSuffix expr)
        {
            Template template = new Template("(<expr><op>)");
            template.Add("op", expr.Op);
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprBin expr)
        {
            Template template = new Template("(<left> <op> <right>)");
            template.Add("left", expr.Left.Accept(this));
            template.Add("right", expr.Right.Accept(this));
            template.Add("op", expr.Op);
            return template;
        }

        public override Template Visit(ExprConst expr)
        {
            return new Template(expr.Text);
        }

        public override Template Visit(ExprBlock block)
        {
            Template template = new Template("({\n    <list; separator=\"\n\">})");
            List<Template> list = new List<Template>();
            foreach (var node in block.StmtList)
            {
                Template expr = new Template("<expr>;");
                expr.Add("expr", node.Accept(this));
                list.Add(expr);
            }
            template.Add("list", list);
            return template;
        }
    }
}
