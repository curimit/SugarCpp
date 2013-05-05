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
            return root.Block.Accept(this);
        }

        public override Template Visit(GlobalBlock block)
        {
            Template template = new Template("<list; separator=\"\n\">");
            List<Template> list = new List<Template>();
            bool last = false;
            AstNode last_node = null;
            foreach (var node in block.List)
            {
                bool current = node is FuncDef || node is Class || node is Enum || node is Import || node is GlobalUsing || node is Namespace;
                if ((last || current) && !(last_node is Import && node is Import))
                {
                    Template tp = new Template("\n<node>");
                    tp.Add("node", node.Accept(this));
                    list.Add(tp);
                }
                else
                {
                    list.Add(node.Accept(this));
                }
                last = current;
                last_node = node;
            }
            template.Add("list", list);
            return template;
        }

        public override Template Visit(GlobalUsing global_using)
        {
            Template template = new Template("using <list; separator=\" \">;");
            template.Add("list", global_using.List);
            return template;
        }

        public override Template Visit(GlobalTypeDef global_typedef)
        {
            Template template = new Template("typedef <type> <name>;");
            template.Add("type", global_typedef.Type);
            template.Add("name", global_typedef.Name);
            return template;
        }

        public override Template Visit(GlobalAlloc global_alloc)
        {
            string prefix = "";
            if (global_alloc.Attribute.Find(x => x.Name == "static") != null)
            {
                prefix += "static ";
            }

            if (global_alloc.Attribute.Find(x => x.Name == "const") != null)
            {
                prefix += "const ";
            }

            if (global_alloc.Expr != null)
            {
                Template template = new Template("<prefix><type> <name; separator=\", \"> = <expr>;");
                template.Add("prefix", prefix);
                template.Add("type", global_alloc.Type);
                template.Add("name", global_alloc.Name);
                template.Add("expr", global_alloc.Expr.Accept(this));
                return template;
            }
            else
            {
                Template template = new Template("<prefix><type> <name; separator=\", \">;");
                template.Add("prefix", prefix);
                template.Add("type", global_alloc.Type);
                template.Add("name", global_alloc.Name);
                return template;
            }
        }

        public override Template Visit(Enum enum_def)
        {
            Template template = new Template("enum <name> {\n    <list; separator=\",\n\">\n};<tostring>");
            template.Add("name", enum_def.Name);
            List<Template> list = new List<Template>();
            bool hasFlagAttribute = enum_def.Attribute.Find(x => x.Name == "FlagAttribute") != null;
            int i = 0;
            foreach (var item in enum_def.Values)
            {
                Template tp = new Template("<node><suffix>");
                tp.Add("node", item);
                if (i == 0 || hasFlagAttribute)
                {
                    tp.Add("suffix", string.Format(" = {0}", i));
                }
                else
                {
                    tp.Add("suffix", "");
                }
                list.Add(tp);
                if (hasFlagAttribute)
                {
                    i = i == 0 ? 1 : i * 2;
                }
                else
                {
                    i = i + 1;
                }
            }
            template.Add("list", list);

            if (enum_def.Attribute.Find(x => x.Name == "ToString") != null)
            {
                Attr attr = enum_def.Attribute.Find(x => x.Name == "ToString");

                FuncDef func = new FuncDef();
                func.Type = "const char*";
                func.Name = attr.Args.Count() == 0 ? "ToString" : attr.Args.First();
                func.Args.Add(new ExprAlloc("const " + enum_def.Name + "&", new List<string> { "a" }, null));
                StmtBlock body = new StmtBlock();
                StmtSwitch stmt_switch = new StmtSwitch();
                stmt_switch.Expr = new ExprConst("a");
                foreach (var item in enum_def.Values)
                {
                    StmtBlock block = new StmtBlock();
                    block.StmtList.Add(new StmtExpr(new ExprReturn(new ExprConst("\"" + item + "\""))));
                    stmt_switch.List.Add(new StmtSwitchItem(new ExprConst(item), block));
                }
                body.StmtList.Add(stmt_switch);
                func.Body = body;
                Template node = new Template("\n\n<stmt>");
                node.Add("stmt", func.Accept(this));
                template.Add("tostring", node);
            }
            else
            {
                template.Add("tostring", "");
            }
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

        public override Template Visit(StmtSwitchItem stmt_switch_item)
        {
            Template template = new Template("case <expr>:\n    <block>");
            template.Add("expr", stmt_switch_item.Expr.Accept(this));
            template.Add("block", stmt_switch_item.Block.Accept(this));
            return template;
        }

        public override Template Visit(StmtSwitch stmt_switch)
        {
            Template template = new Template("switch (<expr>) {\n<list; separator=\"\n\n\">\n}");
            template.Add("expr", stmt_switch.Expr.Accept(this));
            template.Add("list", stmt_switch.List.Select(x => x.Accept(this)));
            return template;
        }

        public override Template Visit(Class class_def)
        {
            Template template = null;
            if (class_def.GenericParameter.Count() == 0)
            {
                template = new Template("class <name><inherit> {\n<list; separator=\"\n\">\n};");
            }
            else
            {
                template = new Template("template \\<<generics; separator=\", \">>\nclass <name><inherit> {\n<list; separator=\"\n\">\n};");
                template.Add("generics", class_def.GenericParameter.Select(x => string.Format("typename {0}", x)));
            }
            template.Add("name", class_def.Name);
            if (class_def.Inherit.Count() > 0)
            {
                Template tmp = new Template(": <inherit; separator=\", \">");
                tmp.Add("inherit", class_def.Inherit.Select(x => string.Format("public {0}", x)));
                template.Add("inherit", tmp);
            }
            List<Template> list = new List<Template>();

            string last = "private";
            bool last_flag = false;
            AstNode last_node = null;

            // friend class
            foreach (var attr in class_def.Attribute)
            {
                if (attr.Name == "friend")
                {
                    foreach (var name in attr.Args)
                    {
                        Template friend = new Template("    friend class <name>;");
                        friend.Add("name", name);
                        list.Add(friend);
                    }
                }
            }

            // Args
            if (class_def.Args.Count() > 0)
            {
                Template tmp = new Template("\npublic:\n    <nodes; separator=\"\n\">\n\n    <constructor>");
                List<Template> nodes = new List<Template>();
                foreach (var item in class_def.Args)
                {
                    GlobalAlloc alloc = new GlobalAlloc(item.Type, item.Name, null, null);
                    nodes.Add(alloc.Accept(this));
                }
                tmp.Add("nodes", nodes);
                list.Add(tmp);

                FuncDef func = new FuncDef();
                func.Type = null;
                func.Name = class_def.Name;
                func.Args = class_def.Args;
                func.Body = new StmtBlock();
                foreach (var item in class_def.Args)
                {
                    string name = item.Name.First();
                    ExprAssign assign = new ExprAssign(new ExprAccess(new ExprConst("this"), "->", name), new ExprConst(name));
                    func.Body.StmtList.Add(new StmtExpr(assign));
                }

                tmp.Add("constructor", func.Accept(this));

                last = "public";
                last_flag = true;
            }

            if (class_def.Block != null)
            {


                foreach (var node in class_def.Block.List)
                {
                    bool current = node is FuncDef || node is Class || node is Enum || node is Import || node is GlobalUsing || node is Namespace;
                    string modifier = node.Attribute.Find(x => x.Name == "public") != null ? "public" : "private";

                    if (modifier != last)
                    {
                        Template member = new Template("\n<modifier>:\n    <expr>");
                        member.Add("modifier", modifier);
                        member.Add("expr", node.Accept(this));
                        list.Add(member);
                    }
                    else
                    {
                        if ((last_flag || current) && !(last_node is Import && node is Import))
                        {
                            Template member = new Template("\n    <expr>");
                            member.Add("expr", node.Accept(this));
                            list.Add(member);
                        }
                        else
                        {
                            Template member = new Template("    <expr>");
                            member.Add("expr", node.Accept(this));
                            list.Add(member);
                        }

                    }

                    last = modifier;
                    last_flag = current;
                    last_node = node;
                }
            }

            template.Add("list", list);
            return template;
        }

        public override Template Visit(Namespace namespace_def)
        {
            Template template = new Template("namespace <name> {\n    <block>\n}");
            string name = namespace_def.Name;
            if (name.IndexOf("::") != -1)
            {
                int k = name.IndexOf("::");
                string prefix = name.Substring(0, k);
                string suffix = name.Substring(k + 2, name.Length - k - 2);
                template.Add("name", prefix);
                template.Add("block", Visit(new Namespace(suffix, namespace_def.Block)));
            }
            else
            {
                template.Add("name", namespace_def.Name);
                List<Template> list = new List<Template>();
                template.Add("block", namespace_def.Block.Accept(this));
            }
            return template;
        }

        public override Template Visit(FuncDef func_def)
        {
            string prefix = "";
            if (func_def.Attribute.Find(x => x.Name == "inline") != null)
            {
                prefix += "inline ";
            }
            if (func_def.Attribute.Find(x => x.Name == "static") != null)
            {
                prefix += "static ";
            }
            string suffix = "";
            if (func_def.Attribute.Find(x => x.Name == "const") != null)
            {
                suffix += " const";
            }

            Template template = null;
            if (func_def.GenericParameter.Count() == 0)
            {
                if (func_def.Type == null)
                {
                    template = new Template("<prefix><name>(<args; separator=\", \">)<suffix> {\n    <list; separator=\"\n\">\n}");
                }
                else
                {
                    template = new Template("<prefix><type> <name>(<args; separator=\", \">)<suffix> {\n    <list; separator=\"\n\">\n}");
                }
            }
            else
            {
                if (func_def.Type == null)
                {
                    template = new Template("template \\<<generics; separator=\", \">>\n<prefix><name>(<args; separator=\", \">)<suffix> {\n    <list; separator=\"\n\">\n}");
                }
                else
                {
                    template = new Template("template \\<<generics; separator=\", \">>\n<prefix><type> <name>(<args; separator=\", \">)<suffix> {\n    <list; separator=\"\n\">\n}");
                }
                template.Add("generics", func_def.GenericParameter.Select(x => string.Format("typename {0}", x)));
            }
            template.Add("prefix", prefix);
            template.Add("suffix", suffix);
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

        public override Template Visit(StmtLinq stmt_linq)
        {
            StmtBlock block = stmt_linq.Block;
            Stmt stmt = null;

            bool isBlock = true;

            // reverse list
            Stack<LinqItem> stack = new Stack<LinqItem>();
            foreach (var item in stmt_linq.List)
            {
                stack.Push(item);
            }

            foreach (var item in stack)
            {
                if (item is LinqFrom)
                {
                    LinqFrom linq_from = (LinqFrom) item;
                    stmt = new StmtForEach(new ExprConst(linq_from.Var), linq_from.Expr, block);
                    block = new StmtBlock();
                    block.StmtList.Add(stmt);
                    isBlock = false;
                }
                if (item is LinqLet)
                {
                    LinqLet linq_let = (LinqLet) item;
                    block.StmtList.Insert(0, new StmtExpr(new ExprAlloc("auto", new List<string> { linq_let.Var }, linq_let.Expr)));
                    isBlock = true;
                }
                if (item is LinqWhere)
                {
                    LinqWhere linq_where = (LinqWhere) item;
                    stmt = new StmtIf(linq_where.Expr, block, null);
                    block = new StmtBlock();
                    block.StmtList.Add(stmt);
                    isBlock = false;    
                }
            }
            if (isBlock)
            {
                Template template = new Template("{\n    <block>\n}");
                template.Add("block", block.Accept(this));
                return template;
            }
            else
            {
                return stmt.Accept(this);
            }
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
                if (expr.Type != "decltype")
                {
                    Template template = new Template("<type> <name; separator=\", \"> = <expr>");
                    template.Add("type", expr.Type);
                    template.Add("name", expr.Name);
                    template.Add("expr", expr.Expr.Accept(this));
                    return template;
                }
                else
                {
                    Template template = new Template("decltype(<expr>) <name; separator=\", \"> = <expr>");
                    template.Add("type", expr.Type);
                    template.Add("name", expr.Name);
                    template.Add("expr", expr.Expr.Accept(this));
                    return template;
                }
            }
            else
            {
                Template template = new Template("<type> <name; separator=\", \">");
                template.Add("type", expr.Type);
                template.Add("name", expr.Name);
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
            Template template = new Template("<expr><list>");
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
