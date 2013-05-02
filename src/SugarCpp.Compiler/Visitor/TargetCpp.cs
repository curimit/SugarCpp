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

        public override Template Visit(StmtUsing stmt_using)
        {
            Template template = new Template("using <list; separator=\" \">");
            template.Add("list", stmt_using.List);
            return template;
        }

        public override Template Visit(StmtTypeDef stmt_typedef)
        {
            Template template = new Template("typedef <type> <name>");
            template.Add("type", stmt_typedef.Type);
            template.Add("name", stmt_typedef.Name);
            return template;
        }

        public override Template Visit(Class class_def)
        {
            Template template = new Template("class <name> {\n<list; separator=\"\n\n\">\n};");
            template.Add("name", class_def.Name);
            List<Template> list = new List<Template>();
            string last = "private";
            foreach (var node in class_def.List)
            {
                string modifier = node.Attribute.Find(x => x.Name == "public") != null ? "public" : "private";
                if (modifier != last)
                {
                    Template member = new Template("<modifier>:\n    <expr>");
                    member.Add("modifier", modifier);
                    member.Add("expr", node.Accept(this));
                    list.Add(member);
                }
                else
                {
                    Template member = new Template("    <expr>");
                    member.Add("expr", node.Accept(this));
                    list.Add(member);
                }
                last = modifier;
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(ClassMember class_member)
        {
            string prefix = "";
            if (class_member.Attribute.Find(x => x.Name == "static") != null)
            {
                prefix += "static ";
            }
            if (class_member.Attribute.Find(x => x.Name == "const") != null)
            {
                prefix += "const ";
            }

            Template template = new Template(string.Format("{0}<node>", prefix));

            template.Add("node", class_member.Node.Accept(this));
            return template;
        }

        public override Template Visit(Namespace namespace_def)
        {
            Template template = new Template("namespace <name> {\n    <list; separator=\"\n\">\n}");
            string name = namespace_def.Name;
            if (name.IndexOf("::") != -1)
            {
                int k = name.IndexOf("::");
                string prefix = name.Substring(0, k);
                string suffix = name.Substring(k + 2, name.Length - k - 2);
                template.Add("name", prefix);
                template.Add("list", Visit(new Namespace(suffix, namespace_def.List)));
            }
            else
            {
                template.Add("name", namespace_def.Name);
                List<Template> list = new List<Template>();
                foreach (var node in namespace_def.List)
                {
                    Template member = new Template("<expr>");
                    member.Add("expr", node.Accept(this));
                    list.Add(member);
                }
                template.Add("list", list);
            }
            return template;
        }

        public override Template Visit(FuncDef func_def)
        {
            Template template = null;
            if (func_def.GenericParameter.Count() == 0)
            {
                template = new Template("<type> <name>(<args; separator=\", \">) {\n    <list; separator=\"\n\">\n}");
            }
            else
            {
                template = new Template("template \\<<generics; separator=\", \">>\n<type> <name>(<args; separator=\", \">) {\n    <list; separator=\"\n\">\n}");
                template.Add("generics", func_def.GenericParameter.Select(x => string.Format("typename {0}", x)));
            }
            template.Add("type", func_def.Type);
            template.Add("name", func_def.Name);
            template.Add("args", func_def.Args.Select(x => x.Accept(this)));
            template.Add("list", func_def.Body.Accept(this));
            return template;
        }

        public override Template Visit(StmtBlock block)
        {
            Template template = new Template("<list; separator=\"\n\">");
            List<Template> list = new List<Template>();
            foreach (var node in block.StmtList)
            {
                Template expr = new Template("<expr>");
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
                Template template = new Template("if (<cond>) {\n    <body>\n}");
                template.Add("cond", stmt_if.Condition.Accept(this));
                template.Add("body", stmt_if.Body.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("if (<cond>) {\n    <body1>\n} else {\n    <body2>\n}");
                template.Add("cond", stmt_if.Condition.Accept(this));
                template.Add("body1", stmt_if.Body.Accept(this));
                template.Add("body2", stmt_if.Else.Accept(this));
                return template;
            }
        }

        public override Template Visit(StmtWhile stmt_while)
        {
            Template template = new Template("while (<cond>) {\n    <body>\n}");
            template.Add("cond", stmt_while.Condition.Accept(this));
            template.Add("body", stmt_while.Body.Accept(this));
            return template;
        }

        public override Template Visit(StmtTry stmt_try)
        {
            Template template = new Template("try {\n    <body>\n} catch (<expr>) {\n    <catch>\n}");
            template.Add("body", stmt_try.Body.Accept(this));
            template.Add("expr", stmt_try.Expr.Accept(this));
            template.Add("catch", stmt_try.Catch.Accept(this));
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

        public override Template Visit(StmtForEach stmt_for_each)
        {
            Template template = new Template("for (auto <var> : <expr>) {\n    <body>\n}");
            template.Add("var", stmt_for_each.Var);
            template.Add("expr", stmt_for_each.Target.Accept(this));
            template.Add("body", stmt_for_each.Body.Accept(this));
            return template;
        }

        public override Template Visit(StmtExpr stmt)
        {
            Template template = new Template("<stmt>;");
            template.Add("stmt", stmt.Stmt.Accept(this));
            return template;
        }

        public override Template Visit(ExprBracket expr)
        {
            Template template = new Template("(<expr>)");
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprAlloc expr)
        {
            if (expr.Expr != null)
            {
                Template template = new Template("<type> <name; separator=\", \"> = <expr>");
                template.Add("type", expr.Type);
                template.Add("name", expr.Name.Select(x => x.Accept(this)));
                template.Add("expr", expr.Expr.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("<type> <name; separator=\", \">");
                template.Add("type", expr.Type);
                template.Add("name", expr.Name.Select(x => x.Accept(this)));
                return template;
            }
        }

        public override Template Visit(MatchTuple match)
        {
            Template template = new Template("std::tie(<list; separator=\", \">)");
            template.Add("list", match.ExprList.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprAssign expr)
        {
            Template template = new Template("<left> = <right>");
            template.Add("left", expr.Left.Accept(this));
            template.Add("right", expr.Right.Accept(this));
            return template;
        }

        public override Template Visit(ExprLambda expr)
        {
            Template template = new Template("([](<args; separator=\",\">) { return <expr>; })");
            template.Add("args", expr.Args.Select(x => x.Accept(this)));
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }
        public override Template Visit(ExprTuple expr)
        {
            Template template = new Template("std::make_tuple(<list; separator=\", \">)");
            template.Add("list", expr.ExprList.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprCall expr)
        {
            if (expr.GenericParameter.Count() == 0)
            {
                Template template = new Template("<expr>(<args; separator=\", \">)");
                template.Add("expr", expr.Expr.Accept(this));
                template.Add("args", expr.Args.Select(x => x.Accept(this)));
                return template;
            }
            else
            {
                Template template = new Template("<expr>\\<<generics; separator=\", \">>(<args; separator=\", \">)");
                template.Add("expr", expr.Expr.Accept(this));
                template.Add("generics", expr.GenericParameter);
                template.Add("args", expr.Args.Select(x => x.Accept(this)));
                return template;
            }
        }

        public override Template Visit(ExprCond expr)
        {
            Template template = new Template("<cond> ? <expr1> : <expr2>");
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
            Template template = new Template("(*<expr>)<list>");
            template.Add("expr", expr.Expr.Accept(this));
            List<Template> list = new List<Template>();
            foreach (var index in expr.Index)
            {
                Template item = new Template("[<index>]");
                item.Add("index", index.Accept(this));
                list.Add(item);
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(ExprNewType expr)
        {
            Template template = new Template("new <elem>(<args; separator=\", \">)");
            template.Add("elem", expr.ElemType);
            template.Add("args", expr.Args.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprNewArray expr)
        {
            Template template = new Template("new <elem>[<list; separator=\", \">]");
            template.Add("elem", expr.ElemType);
            template.Add("list", expr.Args.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(ExprAccess expr)
        {
            Template template = new Template("<expr><op><name>");
            template.Add("expr", expr.Expr.Accept(this));
            template.Add("op", expr.Op);
            template.Add("name", expr.Name);
            return template;
        }

        public override Template Visit(ExprPrefix expr)
        {
            Template template = new Template("<op><expr>");
            template.Add("op", expr.Op);
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprSuffix expr)
        {
            Template template = new Template("<expr><op>");
            template.Add("op", expr.Op);
            template.Add("expr", expr.Expr.Accept(this));
            return template;
        }

        public override Template Visit(ExprBin expr)
        {
            Template template = new Template("<left> <op> <right>");
            template.Add("left", expr.Left.Accept(this));
            template.Add("right", expr.Right.Accept(this));
            template.Add("op", expr.Op);
            return template;
        }

        public override Template Visit(ExprInfix expr)
        {
            Template template = new Template("<func>(<left>, <right>)");
            template.Add("func", expr.Func);
            template.Add("left", expr.Left.Accept(this));
            template.Add("right", expr.Right.Accept(this));
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
